using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class SpecManagementService : ISpecManagementService
{
    private const int UsageExamples = 5;

    private readonly ISpecRepository _specRepository;
    private readonly IProductManagementRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpecManagementService(
        ISpecRepository specRepository,
        IProductManagementRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _specRepository = specRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpecManagementViewModel> GetAsync(int? categoryId, CancellationToken cancellationToken = default)
    {
        var categories = await _productRepository.GetCategoriesAsync(cancellationToken);
        var selected = categories.FirstOrDefault(category => category.Id == categoryId) ?? categories.FirstOrDefault();
        var model = new SpecManagementViewModel
        {
            Categories = categories.Select(category => new ManagementOptionViewModel
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            }).ToList()
        };
        if (selected is null)
        {
            return model;
        }

        var attributes = await _specRepository.GetAttributesAsync(selected.Id, cancellationToken);
        var usage = await _specRepository.CountOptionUsageAsync(selected.Id, cancellationToken);
        var products = await _productRepository.GetManagementListAsync(null, selected.Id, null, null, cancellationToken);

        return new SpecManagementViewModel
        {
            Categories = model.Categories,
            CategoryId = selected.Id,
            CategoryName = selected.Name,
            CategoryProductCount = products.Count,
            Attributes = attributes.Select(attribute => new SpecAttributeItemViewModel
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Hint = attribute.Hint,
                Scope = attribute.Scope,
                UsageCount = attribute.Options.Sum(option => usage.GetValueOrDefault(option.Id)),
                Options = attribute.Options.Select(option => new SpecOptionItemViewModel
                {
                    Id = option.Id,
                    Value = option.Value,
                    Hint = option.Hint,
                    UsageCount = usage.GetValueOrDefault(option.Id)
                }).ToList()
            }).ToList()
        };
    }

    #region Attributes

    public async Task<int> CreateAttributeAsync(SpecAttributeFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!await _productRepository.CategoryExistsAsync(form.CategoryId, cancellationToken))
        {
            throw new KeyNotFoundException();
        }

        var name = form.Name.Trim();
        if (await _specRepository.AttributeNameExistsAsync(form.CategoryId, name, null, cancellationToken))
        {
            throw new InvalidOperationException($"Danh mục đã có thông số \"{name}\".");
        }

        var existing = await _specRepository.GetAttributesTrackedAsync(form.CategoryId, cancellationToken);
        await _specRepository.AddAttributeAsync(new SpecAttribute
        {
            CategoryId = form.CategoryId,
            Name = name,
            Hint = NormalizeOptional(form.Hint),
            Scope = form.Scope,
            SortOrder = existing.Count == 0 ? 1 : existing.Max(attribute => attribute.SortOrder) + 1
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return form.CategoryId;
    }

    public async Task<int> UpdateAttributeAsync(SpecAttributeFormViewModel form, CancellationToken cancellationToken = default)
    {
        var attribute = await _specRepository.GetAttributeTrackedAsync(form.Id ?? 0, cancellationToken)
            ?? throw new KeyNotFoundException();
        var name = form.Name.Trim();
        if (await _specRepository.AttributeNameExistsAsync(attribute.CategoryId, name, attribute.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Danh mục đã có thông số \"{name}\".");
        }

        if (form.Scope != attribute.Scope)
        {
            // Values are stored on products or on variants depending on the scope: switching would orphan them.
            var used = await GetUsageAsync(attribute.CategoryId, attribute.Options.Select(option => option.Id), cancellationToken);
            if (used > 0)
            {
                throw new InvalidOperationException(
                    $"Không thể đổi phạm vi của \"{attribute.Name}\" vì đang được gán {used} lần. Hãy gỡ thông số này khỏi sản phẩm/phiên bản trước.");
            }

            attribute.Scope = form.Scope;
        }

        attribute.Name = name;
        attribute.Hint = NormalizeOptional(form.Hint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attribute.CategoryId;
    }

    public async Task<int> DeleteAttributeAsync(int attributeId, CancellationToken cancellationToken = default)
    {
        var attribute = await _specRepository.GetAttributeTrackedAsync(attributeId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var optionIds = attribute.Options.Select(option => option.Id).ToList();
        await EnsureUnusedAsync(attribute.CategoryId, optionIds, $"thông số \"{attribute.Name}\"", cancellationToken);

        _specRepository.RemoveAttribute(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attribute.CategoryId;
    }

    public async Task<int> MoveAttributeAsync(int attributeId, bool up, CancellationToken cancellationToken = default)
    {
        var attribute = await _specRepository.GetAttributeTrackedAsync(attributeId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var siblings = await _specRepository.GetAttributesTrackedAsync(attribute.CategoryId, cancellationToken);
        Move(siblings, siblings.FindIndex(item => item.Id == attributeId), up, (item, order) => item.SortOrder = order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attribute.CategoryId;
    }

    #endregion

    #region Options

    public async Task<int> CreateOptionAsync(SpecOptionFormViewModel form, CancellationToken cancellationToken = default)
    {
        var attribute = await _specRepository.GetAttributeTrackedAsync(form.AttributeId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var value = form.Value.Trim();
        if (await _specRepository.OptionValueExistsAsync(attribute.Id, value, null, cancellationToken))
        {
            throw new InvalidOperationException($"Thông số \"{attribute.Name}\" đã có giá trị \"{value}\".");
        }

        await _specRepository.AddOptionAsync(new SpecOption
        {
            SpecAttributeId = attribute.Id,
            Value = value,
            Hint = NormalizeOptional(form.Hint),
            SortOrder = attribute.Options.Count == 0 ? 1 : attribute.Options.Max(option => option.SortOrder) + 1
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attribute.CategoryId;
    }

    public async Task<int> UpdateOptionAsync(SpecOptionFormViewModel form, CancellationToken cancellationToken = default)
    {
        var option = await _specRepository.GetOptionTrackedAsync(form.Id ?? 0, cancellationToken)
            ?? throw new KeyNotFoundException();
        var value = form.Value.Trim();
        if (await _specRepository.OptionValueExistsAsync(option.SpecAttributeId, value, option.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Thông số \"{option.Attribute!.Name}\" đã có giá trị \"{value}\".");
        }

        // Renaming is safe: products reference the option by id, so they follow the new name.
        option.Value = value;
        option.Hint = NormalizeOptional(form.Hint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return option.Attribute!.CategoryId;
    }

    public async Task<int> DeleteOptionAsync(int optionId, CancellationToken cancellationToken = default)
    {
        var option = await _specRepository.GetOptionTrackedAsync(optionId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var categoryId = option.Attribute!.CategoryId;
        await EnsureUnusedAsync(categoryId, [option.Id], $"giá trị \"{option.Value}\"", cancellationToken);

        _specRepository.RemoveOption(option);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return categoryId;
    }

    public async Task<int> MoveOptionAsync(int optionId, bool up, CancellationToken cancellationToken = default)
    {
        var option = await _specRepository.GetOptionTrackedAsync(optionId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var siblings = option.Attribute!.Options.OrderBy(item => item.SortOrder).ThenBy(item => item.Id).ToList();
        Move(siblings, siblings.FindIndex(item => item.Id == optionId), up, (item, order) => item.SortOrder = order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return option.Attribute.CategoryId;
    }

    #endregion

    #region Helpers

    private async Task<int> GetUsageAsync(int categoryId, IEnumerable<int> optionIds, CancellationToken cancellationToken)
    {
        var usage = await _specRepository.CountOptionUsageAsync(categoryId, cancellationToken);
        return optionIds.Sum(id => usage.GetValueOrDefault(id));
    }

    /// <summary>Blocks deleting definitions still assigned to products, naming a few of them so the admin can fix it.</summary>
    private async Task EnsureUnusedAsync(int categoryId, IReadOnlyCollection<int> optionIds, string label, CancellationToken cancellationToken)
    {
        var used = await GetUsageAsync(categoryId, optionIds, cancellationToken);
        if (used == 0)
        {
            return;
        }

        var names = await _specRepository.GetProductNamesUsingAsync(optionIds, UsageExamples, cancellationToken);
        throw new InvalidOperationException(
            $"Không thể xóa {label} vì đang được gán {used} lần, ví dụ: {string.Join(", ", names)}. " +
            "Hãy bỏ chọn thông số này trong các sản phẩm/phiên bản đó trước.");
    }

    /// <summary>Renumbers 1..n, then swaps the item with its neighbour.</summary>
    private static void Move<T>(List<T> items, int index, bool up, Action<T, int> setOrder)
    {
        var target = up ? index - 1 : index + 1;
        if (index < 0 || target < 0 || target >= items.Count)
        {
            return;
        }

        (items[index], items[target]) = (items[target], items[index]);
        for (var position = 0; position < items.Count; position++)
        {
            setOrder(items[position], position + 1);
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion
}
