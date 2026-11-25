using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Services.Interfaces
{
    public interface IProductService : IService<Product, int>
    {
        Task<IReadOnlyList<ProductCardViewModel>> GetLatestAvailableAsync(
            int count,
            CancellationToken cancellationToken = default);

        /// <summary>Live results for the header search panel (keyword + category, brand, price and specifications).</summary>
        Task<SearchResultsViewModel> SearchAsync(
            int? categoryId,
            int? brandId,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            IReadOnlyCollection<int>? specIds,
            int count,
            string catalogPath,
            CancellationToken cancellationToken = default);

        /// <summary>Brands and specification filters (with counts) of a category, for the header search panel.</summary>
        Task<SearchFiltersViewModel> GetSearchFiltersAsync(int? categoryId, CancellationToken cancellationToken = default);

        Task<ProductCatalogViewModel> GetCatalogAsync(
            int pageNumber,
            int? categoryId,
            int? brandId,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            IReadOnlyCollection<int>? specIds = null,
            CancellationToken cancellationToken = default);
        Task<ProductDetailsViewModel?> GetDetailsAsync(int productId, CancellationToken cancellationToken = default);
        Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken = default);
    }
}
