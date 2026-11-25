namespace PopPop.Services.Interfaces;

public interface IProductImageStorage
{
    /// <summary>
    /// Returns a user-facing error message when the file is not an acceptable product image, otherwise null.
    /// </summary>
    Task<string?> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the image and returns its public URL (e.g. /uploads/products/abc.jpg).
    /// </summary>
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image previously saved by this storage. URLs outside the upload folder (seed images) are ignored.
    /// </summary>
    void Delete(string imageUrl);
}
