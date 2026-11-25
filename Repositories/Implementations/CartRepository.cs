using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class CartRepository : Repository<Cart, int>, ICartRepository
{
    public CartRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<Cart?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsSplitQuery()
            // Category/Brand are needed to know whether each product is still for sale.
            .Include(cart => cart.CartItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.Category)
            .Include(cart => cart.CartItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.Brand)
            .Include(cart => cart.CartItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.ProductImages
                            .OrderByDescending(image => image.IsPrimary)
                            .ThenBy(image => image.SortOrder))
            .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
    }
}
