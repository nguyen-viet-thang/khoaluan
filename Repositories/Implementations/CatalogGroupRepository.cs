using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class CatalogGroupRepository<TEntity> : Repository<TEntity, int>, ICatalogGroupRepository<TEntity>
    where TEntity : class, ICatalogGroupEntity
{
    public CatalogGroupRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<(TEntity Entity, int ProductCount)>> GetListAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(entity => entity.Name.Contains(keyword));
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        var rows = await query
            .OrderBy(entity => entity.Name)
            .Select(entity => new { Entity = entity, ProductCount = entity.Products.Count })
            .ToListAsync(cancellationToken);

        return rows.Select(row => (row.Entity, row.ProductCount)).ToList();
    }

    public async Task<int> CountProductsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(entity => entity.Id == id)
            .Select(entity => entity.Products.Count)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsNameTakenAsync(string name, int? excludeId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            entity => entity.Name == name && (!excludeId.HasValue || entity.Id != excludeId.Value),
            cancellationToken);
    }
}
