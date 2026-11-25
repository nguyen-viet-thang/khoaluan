using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class CategoriesController : CatalogGroupController<Category>
{
    public CategoriesController(ICatalogGroupService<Category> service)
        : base(service)
    {
    }

    protected override string GroupLabel => "danh mục";
    protected override string PageTitle => "Quản lý danh mục";
    protected override string IconClass => "fa-folder-open";
}
