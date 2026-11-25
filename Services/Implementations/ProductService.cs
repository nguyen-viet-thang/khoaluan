using PopPop.Services.Interfaces;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Services.Implementations
{
    public class ProductService : Service<Product, int>, IProductService
    {
        private const int CatalogPageSize = 12;
        private readonly IProductRepository _productRepository;
        private readonly ISpecRepository _specRepository;

        public ProductService(IProductRepository productRepository, ISpecRepository specRepository, IUnitOfWork unitOfWork)
            : base(productRepository, unitOfWork)
        {
            _productRepository = productRepository;
            _specRepository = specRepository;
        }

        public async Task<IReadOnlyList<ProductCardViewModel>> GetLatestAvailableAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.GetLatestAvailableAsync(count, cancellationToken);
            return MapToProductCards(products);
        }

        public async Task<ProductCatalogViewModel> GetCatalogAsync(
            int pageNumber,
            int? categoryId,
            int? brandId,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            IReadOnlyCollection<int>? specIds = null,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Clamp(pageNumber, 1, int.MaxValue / CatalogPageSize);
            var normalizedSort = sortBy is "price-asc" or "price-desc" or "name" ? sortBy : "newest";
            var (filter, attributes) = await BuildFilterAsync(categoryId, brandId, search, minPrice, maxPrice, specIds, cancellationToken);

            var (products, totalCount) = await _productRepository.GetCatalogPageAsync(
                pageNumber, CatalogPageSize, filter, normalizedSort, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)CatalogPageSize);
            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
                (products, totalCount) = await _productRepository.GetCatalogPageAsync(
                    pageNumber, CatalogPageSize, filter, normalizedSort, cancellationToken);
            }

            var categories = await _productRepository.GetActiveCategoriesAsync(cancellationToken);
            var brands = await _productRepository.GetActiveBrandsAsync(cancellationToken);
            var catalogMaxPrice = await _productRepository.GetCatalogMaxPriceAsync(cancellationToken);
            return new ProductCatalogViewModel
            {
                Products = MapToProductCards(products),
                Categories = categories.Select(category => new ProductCategoryOptionViewModel
                {
                    Id = category.Id,
                    Name = category.Name
                }).ToList(),
                Brands = brands.Select(brand => new ProductCategoryOptionViewModel
                {
                    Id = brand.Id,
                    Name = brand.Name
                }).ToList(),
                CategoryId = filter.CategoryId,
                BrandId = filter.BrandId,
                Search = filter.Search,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                PriceRangeMax = Math.Max(1000m, Math.Ceiling(catalogMaxPrice / 1000m) * 1000m),
                SortBy = normalizedSort,
                PageNumber = pageNumber,
                PageSize = CatalogPageSize,
                TotalCount = totalCount,
                SpecFilters = await BuildSpecFiltersAsync(filter, attributes, cancellationToken),
                SelectedSpecIds = filter.Specs.SelectMany(group => group.OptionIds).ToList()
            };
        }

        public async Task<SearchResultsViewModel> SearchAsync(
            int? categoryId,
            int? brandId,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            IReadOnlyCollection<int>? specIds,
            int count,
            string catalogPath,
            CancellationToken cancellationToken = default)
        {
            var (filter, _) = await BuildFilterAsync(categoryId, brandId, search, minPrice, maxPrice, specIds, cancellationToken);
            var url = new CatalogQuery
            {
                CategoryId = filter.CategoryId,
                BrandId = filter.BrandId,
                Q = filter.Search,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                Spec = filter.Specs.SelectMany(group => group.OptionIds).ToList()
            }.ToUrl(catalogPath);

            // Nothing typed or chosen yet: no results (the panel shows its hint instead).
            if (filter.Search is null && filter.CategoryId is null && filter.BrandId is null
                && filter.MinPrice is null && filter.MaxPrice is null)
            {
                return new SearchResultsViewModel { CatalogUrl = url };
            }

            var (products, totalCount) = await _productRepository.GetCatalogPageAsync(
                1, Math.Clamp(count, 1, 20), filter, "name", cancellationToken);
            return new SearchResultsViewModel
            {
                Products = MapToProductCards(products),
                TotalCount = totalCount,
                CatalogUrl = url
            };
        }

        public async Task<SearchFiltersViewModel> GetSearchFiltersAsync(int? categoryId, CancellationToken cancellationToken = default)
        {
            var (filter, attributes) = await BuildFilterAsync(categoryId, null, null, null, null, null, cancellationToken);
            var brands = await _productRepository.GetBrandsWithProductsAsync(filter.CategoryId, cancellationToken);
            var categories = await _productRepository.GetCategorySummariesAsync(cancellationToken);
            return new SearchFiltersViewModel
            {
                CategoryId = filter.CategoryId,
                Categories = categories.Select(category => new ProductCategoryOptionViewModel { Id = category.Id, Name = category.Name }).ToList(),
                Brands = brands.Select(brand => new ProductCategoryOptionViewModel { Id = brand.Id, Name = brand.Name }).ToList(),
                Specs = await BuildSpecFiltersAsync(filter, attributes, cancellationToken)
            };
        }

        /// <summary>
        /// Normalises the raw query: swaps a reversed price range and keeps only option ids that belong to the
        /// selected category (specifications are meaningless without a category).
        /// </summary>
        private async Task<(CatalogFilter Filter, IReadOnlyList<SpecAttribute> Attributes)> BuildFilterAsync(
            int? categoryId,
            int? brandId,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            IReadOnlyCollection<int>? specIds,
            CancellationToken cancellationToken)
        {
            minPrice = minPrice >= 0 ? minPrice : null;
            maxPrice = maxPrice >= 0 ? maxPrice : null;
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            categoryId = categoryId > 0 ? categoryId : null;
            brandId = brandId > 0 ? brandId : null;
            var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            if (normalizedSearch?.Length > 100)
            {
                normalizedSearch = normalizedSearch[..100];
            }

            IReadOnlyList<SpecAttribute> attributes = categoryId.HasValue
                ? await _specRepository.GetAttributesAsync(categoryId.Value, cancellationToken)
                : [];
            var selected = specIds?.ToHashSet() ?? [];
            var groups = attributes
                .Select(attribute => new SpecFilterGroup(
                    attribute.Id,
                    attribute.Scope,
                    attribute.Options.Where(option => selected.Contains(option.Id)).Select(option => option.Id).ToList()))
                .Where(group => group.OptionIds.Count > 0)
                .ToList();

            return (new CatalogFilter(categoryId, brandId, normalizedSearch, minPrice, maxPrice, groups), attributes);
        }

        /// <summary>
        /// Facets with counts: each option shows how many products the result would have with it,
        /// keeping every other filter (selections inside the same attribute are OR-ed, so they are left out).
        /// Options with no product are hidden unless already selected.
        /// </summary>
        private async Task<IReadOnlyList<SpecFilterGroupViewModel>> BuildSpecFiltersAsync(
            CatalogFilter filter,
            IReadOnlyList<SpecAttribute> attributes,
            CancellationToken cancellationToken)
        {
            var selected = filter.Specs.SelectMany(group => group.OptionIds).ToHashSet();
            var result = new List<SpecFilterGroupViewModel>(attributes.Count);
            foreach (var attribute in attributes)
            {
                var counts = await _specRepository.CountProductsByOptionAsync(filter.WithoutAttribute(attribute.Id), attribute, cancellationToken);
                var options = attribute.Options
                    .Select(option => new SpecFilterOptionViewModel
                    {
                        Id = option.Id,
                        Value = option.Value,
                        Hint = option.Hint,
                        ProductCount = counts.GetValueOrDefault(option.Id),
                        Selected = selected.Contains(option.Id)
                    })
                    .Where(option => option.ProductCount > 0 || option.Selected)
                    .ToList();
                if (options.Count > 0)
                {
                    result.Add(new SpecFilterGroupViewModel
                    {
                        AttributeId = attribute.Id,
                        Name = attribute.Name,
                        Hint = attribute.Hint,
                        IsVariantLevel = attribute.Scope == SpecScope.Variant,
                        Options = options
                    });
                }
            }

            return result;
        }

        public async Task<ProductDetailsViewModel?> GetDetailsAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetActiveDetailsAsync(productId, cancellationToken);
            if (product is null)
            {
                return null;
            }

            var (productValues, variantValues) = await _specRepository.GetValuesForProductAsync(product.Id, cancellationToken);
            var buyableValues = variantValues.Where(value => value.Variant is { IsActive: true, StockQuantity: > 0 });
            var specs = productValues.Select(value => value.Option!)
                .Concat(buyableValues.Select(value => value.Option!))
                .DistinctBy(option => option.Id)
                // Group by id: no-tracking queries return separate Attribute instances per row.
                .GroupBy(option => option.SpecAttributeId)
                .Select(group => (Attribute: group.First().Attribute!, Options: group))
                .OrderBy(group => group.Attribute.SortOrder)
                .Select(group => new ProductSpecLineViewModel(
                    group.Attribute.Name,
                    group.Attribute.Hint,
                    group.Options
                        .OrderBy(option => option.SortOrder)
                        .Select(option => new ProductSpecValueViewModel(option.Id, option.Value, option.Hint))
                        .ToList()))
                .ToList();

            return new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Specs = specs,
                CategoryName = product.Category?.Name,
                BrandName = product.Brand?.Name,
                ImageUrls = product.ProductImages.Select(image => image.ImageUrl).ToList(),
                Variants = product.ProductVariants.Select(variant => new ProductVariantOptionViewModel
                {
                    Id = variant.Id,
                    Label = string.IsNullOrWhiteSpace(variant.VariantLabel) ? variant.SKU : variant.VariantLabel,
                    SKU = variant.SKU,
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity,
                    IsAvailable = variant.IsActive && variant.StockQuantity > 0
                }).ToList()
            };
        }

        public async Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken = default)
        {
            var latest = await _productRepository.GetLatestAvailableAsync(8, cancellationToken);
            var bestSellers = await _productRepository.GetBestSellingAvailableAsync(8, cancellationToken);
            var categories = await _productRepository.GetCategorySummariesAsync(cancellationToken);
            var brands = await _productRepository.GetBrandSummariesAsync(cancellationToken);

            // Hero: best sellers when the shop has sales, otherwise the newest arrivals.
            var heroSource = bestSellers.Count >= 3 ? bestSellers : latest;
            var categoryNames = categories.ToDictionary(category => category.Id, category => category.Name);

            return new HomeViewModel
            {
                Featured = heroSource.Take(3).Select(product => new HomeFeaturedProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl,
                    CategoryName = categoryNames.GetValueOrDefault(product.CategoryId),
                    Price = product.ProductVariants.Select(variant => variant.Price).DefaultIfEmpty(0m).Min()
                }).ToList(),
                LatestProducts = MapToProductCards(latest),
                BestSellers = MapToProductCards(bestSellers),
                Categories = categories,
                Brands = brands
            };
        }

        private static IReadOnlyList<ProductCardViewModel> MapToProductCards(IEnumerable<Product> products)
        {
            return products.Select(product =>
            {
                var variant = product.ProductVariants
                    .OrderBy(item => item.Price)
                    .ThenBy(item => item.Id)
                    .FirstOrDefault();

                return new ProductCardViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl,
                    Price = variant?.Price ?? 0m
                };
            }).ToList();
        }
    }
} 
