using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class ProductImageStorage : IProductImageStorage
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string PublicPrefix = "/uploads/products/";

    private static readonly Dictionary<string, byte[][]> AllowedSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        [".gif"] = [[0x47, 0x49, 0x46, 0x38]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]]
    };

    private readonly string _uploadDirectory;
    private readonly ILogger<ProductImageStorage> _logger;

    public ProductImageStorage(IWebHostEnvironment environment, ILogger<ProductImageStorage> logger)
    {
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _uploadDirectory = Path.Combine(webRoot, "uploads", "products");
        _logger = logger;
    }

    public async Task<string?> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return $"Tệp {file.FileName} đang trống.";
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return $"Ảnh {file.FileName} vượt quá dung lượng tối đa 5 MB.";
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedSignatures.TryGetValue(extension, out var signatures))
        {
            return $"Ảnh {file.FileName} không đúng định dạng (chỉ nhận JPG, PNG, GIF, WEBP).";
        }

        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        var matches = signatures.Any(signature =>
            read >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature));

        return matches ? null : $"Nội dung tệp {file.FileName} không phải là ảnh hợp lệ.";
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_uploadDirectory);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_uploadDirectory, fileName);

        await using (var output = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        return PublicPrefix + fileName;
    }

    public void Delete(string imageUrl)
    {
        if (!imageUrl.StartsWith(PublicPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);
        var fullPath = Path.Combine(_uploadDirectory, fileName);
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Could not delete product image {ImageUrl}", imageUrl);
        }
    }
}
