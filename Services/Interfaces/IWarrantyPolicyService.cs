using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Admin configuration: receipt-defect window per brand, warranty period per brand × category,
/// and the defect catalogue customers choose from.
/// </summary>
public interface IWarrantyPolicyService
{
    Task<WarrantyPolicyMatrixViewModel> GetMatrixAsync(CancellationToken cancellationToken = default);
    Task UpdateReceiptDefectDaysAsync(int brandId, int days, CancellationToken cancellationToken = default);
    Task<WarrantyPolicyFormViewModel?> GetPolicyFormAsync(int brandId, int categoryId, CancellationToken cancellationToken = default);
    Task SavePolicyAsync(WarrantyPolicyFormViewModel form, CancellationToken cancellationToken = default);
    Task<DefectTypeFormViewModel?> GetDefectFormAsync(int defectTypeId, CancellationToken cancellationToken = default);
    Task SaveDefectAsync(DefectTypeFormViewModel form, CancellationToken cancellationToken = default);
}
