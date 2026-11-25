using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class WarrantyPolicyService : IWarrantyPolicyService
{
    private const int MaxReceiptDefectDays = 60;

    private readonly IWarrantyPolicyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public WarrantyPolicyService(IWarrantyPolicyRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarrantyPolicyMatrixViewModel> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _repository.GetBrandsAsync(cancellationToken);
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        var policies = await _repository.GetPoliciesAsync(cancellationToken);
        var counts = await _repository.GetProductCountsAsync(cancellationToken);
        var defects = await _repository.GetDefectTypesAsync(cancellationToken);

        return new WarrantyPolicyMatrixViewModel
        {
            Brands = brands.Select(brand => new PolicyBrandColumn(brand.Id, brand.Name, brand.ReceiptDefectDays)).ToList(),
            Categories = categories.Select(category => new PolicyCategoryRow(category.Id, category.Name)).ToList(),
            Cells = (from brand in brands
                     from category in categories
                     let policy = policies.FirstOrDefault(row => row.BrandId == brand.Id && row.CategoryId == category.Id)
                     select new WarrantyPolicyCell
                     {
                         BrandId = brand.Id,
                         CategoryId = category.Id,
                         WarrantyDays = policy?.WarrantyDays ?? 0,
                         CoveredParts = policy?.CoveredParts,
                         Notes = policy?.Notes,
                         SourceUrl = policy?.SourceUrl,
                         ProductCount = counts.GetValueOrDefault((brand.Id, category.Id))
                     }).ToList(),
            Defects = defects.Select(defect => new DefectAdminRow
            {
                Id = defect.Id,
                Code = defect.Code,
                CategoryName = defect.Category?.Name ?? string.Empty,
                Name = defect.Name,
                Coverage = defect.Coverage,
                Source = defect.Source,
                IsActive = defect.IsActive
            }).ToList()
        };
    }

    public async Task UpdateReceiptDefectDaysAsync(int brandId, int days, CancellationToken cancellationToken = default)
    {
        if (days < 0 || days > MaxReceiptDefectDays)
        {
            throw new InvalidOperationException($"Thời hạn báo lỗi khi nhận hàng phải từ 0 đến {MaxReceiptDefectDays} ngày.");
        }

        var brand = await _repository.GetBrandAsync(brandId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy thương hiệu.");
        brand.ReceiptDefectDays = days;
        brand.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<WarrantyPolicyFormViewModel?> GetPolicyFormAsync(int brandId, int categoryId, CancellationToken cancellationToken = default)
    {
        var brand = await _repository.GetBrandAsync(brandId, cancellationToken);
        var category = await _repository.GetCategoryAsync(categoryId, cancellationToken);
        if (brand is null || category is null)
        {
            return null;
        }

        var policy = await _repository.GetPolicyAsync(brandId, categoryId, cancellationToken);
        return new WarrantyPolicyFormViewModel
        {
            BrandId = brandId,
            CategoryId = categoryId,
            BrandName = brand.Name,
            CategoryName = category.Name,
            WarrantyDays = policy?.WarrantyDays ?? 0,
            CoveredParts = policy?.CoveredParts,
            Notes = policy?.Notes,
            SourceUrl = policy?.SourceUrl
        };
    }

    public async Task SavePolicyAsync(WarrantyPolicyFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (await _repository.GetBrandAsync(form.BrandId, cancellationToken) is null
            || await _repository.GetCategoryAsync(form.CategoryId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("Không tìm thấy thương hiệu hoặc danh mục.");
        }

        var policy = await _repository.GetPolicyAsync(form.BrandId, form.CategoryId, cancellationToken);
        if (policy is null)
        {
            policy = new WarrantyPolicy { BrandId = form.BrandId, CategoryId = form.CategoryId };
            await _repository.AddPolicyAsync(policy, cancellationToken);
        }

        policy.WarrantyDays = form.WarrantyDays;
        policy.CoveredParts = Normalize(form.CoveredParts);
        policy.Notes = Normalize(form.Notes);
        policy.SourceUrl = Normalize(form.SourceUrl);
        policy.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DefectTypeFormViewModel?> GetDefectFormAsync(int defectTypeId, CancellationToken cancellationToken = default)
    {
        var defect = await _repository.GetDefectTypeAsync(defectTypeId, cancellationToken);
        return defect is null ? null : new DefectTypeFormViewModel
        {
            Id = defect.Id,
            Code = defect.Code,
            CategoryName = defect.Category?.Name ?? string.Empty,
            Name = defect.Name,
            Recognition = defect.Recognition,
            EvidenceGuide = defect.EvidenceGuide,
            Coverage = defect.Coverage,
            Source = defect.Source,
            SourceUrl = defect.SourceUrl,
            IsActive = defect.IsActive
        };
    }

    public async Task SaveDefectAsync(DefectTypeFormViewModel form, CancellationToken cancellationToken = default)
    {
        var defect = await _repository.GetDefectTypeAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy loại lỗi.");
        defect.Name = form.Name.Trim();
        defect.Recognition = Normalize(form.Recognition);
        defect.EvidenceGuide = Normalize(form.EvidenceGuide);
        defect.Coverage = form.Coverage;
        defect.Source = Normalize(form.Source);
        defect.SourceUrl = Normalize(form.SourceUrl);
        defect.IsActive = form.IsActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
