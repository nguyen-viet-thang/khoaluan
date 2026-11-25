using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public abstract class CatalogGroupService<TEntity> : ICatalogGroupService<TEntity>
    where TEntity : class, ICatalogGroupEntity
{
    private readonly ICatalogGroupRepository<TEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;

    protected CatalogGroupService(ICatalogGroupRepository<TEntity> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Lower-case Vietnamese noun used in messages, e.g. "danh mục".</summary>
    protected abstract string GroupLabel { get; }

    protected abstract TEntity CreateEntity(string name);

    public async Task<CatalogGroupListViewModel> GetListAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = status is "active" or "inactive" ? status : null;
        bool? isActive = normalizedStatus switch
        {
            "active" => true,
            "inactive" => false,
            _ => null
        };
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var rows = await _repository.GetListAsync(normalizedSearch, isActive, cancellationToken);
        return new CatalogGroupListViewModel
        {
            Items = rows.Select(row => new CatalogGroupItemViewModel
            {
                Id = row.Entity.Id,
                Name = row.Entity.Name,
                Description = row.Entity.Description,
                IsActive = row.Entity.IsActive,
                ProductCount = row.ProductCount,
                UpdatedAt = row.Entity.UpdatedAt
            }).ToList(),
            Search = normalizedSearch,
            Status = normalizedStatus
        };
    }

    public async Task<CatalogGroupFormViewModel?> GetFormAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return new CatalogGroupFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            ProductCount = await _repository.CountProductsAsync(id, cancellationToken)
        };
    }

    public async Task CreateAsync(CatalogGroupFormViewModel form, CancellationToken cancellationToken = default)
    {
        var name = await EnsureUniqueNameAsync(form.Name, null, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = CreateEntity(name);
        entity.Description = NormalizeOptional(form.Description);
        entity.IsActive = form.IsActive;
        entity.CreatedAt = now;
        entity.UpdatedAt = now;

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CatalogGroupFormViewModel form, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(form.Id ?? 0, cancellationToken)
            ?? throw new KeyNotFoundException();

        entity.Name = await EnsureUniqueNameAsync(form.Name, entity.Id, cancellationToken);
        entity.Description = NormalizeOptional(form.Description);
        entity.IsActive = form.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException();

        var productCount = await _repository.CountProductsAsync(id, cancellationToken);
        if (productCount > 0)
        {
            throw new InvalidOperationException(
                $"Không thể xóa {GroupLabel} \"{entity.Name}\" vì đang có {productCount} sản phẩm. " +
                $"Hãy chuyển các sản phẩm sang {GroupLabel} khác hoặc tắt \"Đang hoạt động\".");
        }

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> EnsureUniqueNameAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        if (await _repository.IsNameTakenAsync(normalizedName, excludeId, cancellationToken))
        {
            throw new InvalidOperationException($"Tên {GroupLabel} \"{normalizedName}\" đã tồn tại.");
        }

        return normalizedName;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class CategoryManagementService : CatalogGroupService<Category>
{
    public CategoryManagementService(ICatalogGroupRepository<Category> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }

    protected override string GroupLabel => "danh mục";

    protected override Category CreateEntity(string name) => new() { Name = name };
}

public sealed class BrandManagementService : CatalogGroupService<Brand>
{
    public BrandManagementService(ICatalogGroupRepository<Brand> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }

    protected override string GroupLabel => "thương hiệu";

    protected override Brand CreateEntity(string name) => new() { Name = name };
}
