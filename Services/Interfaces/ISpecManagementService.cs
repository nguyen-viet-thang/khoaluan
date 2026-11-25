using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Admin management of specification definitions (attributes and their options) per category.
/// Rule violations throw <see cref="InvalidOperationException"/> with a user-facing message;
/// missing records throw <see cref="KeyNotFoundException"/>. Methods return the category id to go back to.
/// </summary>
public interface ISpecManagementService
{
    Task<SpecManagementViewModel> GetAsync(int? categoryId, CancellationToken cancellationToken = default);

    Task<int> CreateAttributeAsync(SpecAttributeFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> UpdateAttributeAsync(SpecAttributeFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> DeleteAttributeAsync(int attributeId, CancellationToken cancellationToken = default);
    Task<int> MoveAttributeAsync(int attributeId, bool up, CancellationToken cancellationToken = default);

    Task<int> CreateOptionAsync(SpecOptionFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> UpdateOptionAsync(SpecOptionFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> DeleteOptionAsync(int optionId, CancellationToken cancellationToken = default);
    Task<int> MoveOptionAsync(int optionId, bool up, CancellationToken cancellationToken = default);
}
