using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Back-office management of product groupings (Category, Brand).
/// Business-rule violations throw <see cref="InvalidOperationException"/> with a user-facing message;
/// missing records throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface ICatalogGroupService<TEntity>
    where TEntity : class, ICatalogGroupEntity
{
    Task<CatalogGroupListViewModel> GetListAsync(string? search, string? status, CancellationToken cancellationToken = default);
    Task<CatalogGroupFormViewModel?> GetFormAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(CatalogGroupFormViewModel form, CancellationToken cancellationToken = default);
    Task UpdateAsync(CatalogGroupFormViewModel form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
