using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class BrandsController : CatalogGroupController<Brand>
{
    public BrandsController(ICatalogGroupService<Brand> service)
        : base(service)
    {
    }

    protected override string GroupLabel => "thương hiệu";
    protected override string PageTitle => "Quản lý thương hiệu";
    protected override string IconClass => "fa-tags";
}
