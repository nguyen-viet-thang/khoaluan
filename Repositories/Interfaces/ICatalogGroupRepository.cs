using PopPop.Data.Entities;

namespace PopPop.Repositories.Interfaces;

/// <summary>
/// Data access for product groupings (Category, Brand).
/// </summary>
public interface ICatalogGroupRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class, ICatalogGroupEntity
{
    Task<IReadOnlyList<(TEntity Entity, int ProductCount)>> GetListAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);
    Task<int> CountProductsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsNameTakenAsync(string name, int? excludeId, CancellationToken cancellationToken = default);
}
