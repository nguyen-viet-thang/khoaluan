# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

PopPop: an ASP.NET Core MVC (.NET 10) e-commerce + management site for a badminton shop (graduation thesis project). UI text, seed data, and docs are in Vietnamese; code and comments are in English. Working branch is `thang`.

## Commands

```powershell
dotnet build
dotnet run                     # http://localhost:5263 (https profile: https://localhost:7253)
dotnet ef database update      # apply migrations + seed data (needs: dotnet tool install --global dotnet-ef)
dotnet ef migrations add <Name> --output-dir Data/Migrations
```

- There is no test project; CI (`.github/workflows/dotnet.yml`) only restores/builds (and runs `dotnet test`, which finds nothing). Verify changes by building and running the app.
- `appsettings.json` is git-ignored and must be created locally — see README for the template (`ConnectionStrings:DefaultConnection` → LocalDB `PopPopShop`, `Smtp`, `Shipping`, optional `Store`, `Payment:VnPay`). With empty `Smtp.Host`, emails aren't sent and confirmation/reset links are shown on the page in Development.
- Swagger UI is enabled in Development.
- Seed accounts (dev only): Admin `0900000000` / `Admin@123` and Staff `0911222301` / `PopPop@123` log in at `/admin/login`; customer `0901234501` / `PopPop@123` at `/Identity/Account/Login`.
- Extra test data scripts (run against LocalDB): `powershell -ExecutionPolicy Bypass -File Docs\Seed-DeliveredOrders.ps1` and `Docs\Seed-WarrantyClaims.ps1`.
- `wwwroot/scss/main.scss` compiles to `wwwroot/scss/main.css` via the VS Web Compiler extension (`compilerconfig.json`).

## Architecture

N-tier: Controllers → Services → Repositories → EF Core (`PopPopDbContext`, SQL Server). All DI registration is in `Program.cs` — register every new repository/service there.

- **Repositories** (`Repositories/Interfaces` + `Implementations`): generic `IRepository<TEntity,TKey>` for entities implementing `IBaseEntity<TKey>`, plus feature repositories (Product, Order, Cart, Report, WarrantyClaim, …). Repositories don't save; `IUnitOfWork.SaveChangesAsync` does. Multi-step writes that touch stock (checkout, cancel, inventory) use `IUnitOfWork.ExecuteInTransactionAsync` (Serializable isolation, saves + commits for you).
- **Services** (`Services/Interfaces` + `Implementations`): business logic. `Service<TEntity,TKey>` is a CRUD base. Categories and Brands share a generic catalog-group stack (`ICatalogGroupEntity`, `CatalogGroupRepository<T>`, `ICatalogGroupService<T>`, `CatalogGroupController` base in Admin, shared views in `Areas/Admin/Views/Shared/CatalogGroup/`).
- `PopPopDbContext` is registered via `AddDbContextFactory` (scoped context also available). `HeaderBadgeService` uses the factory to create short-lived contexts for layout badges, and serves the storefront header's category/brand mega menu (`GetMenuAsync`, cached 5 min in `IMemoryCache`).
- **Seed data** lives in `Data/Seed/*` and is applied with `HasData` from `OnModelCreating` — changing seed data requires a new migration.
- **Time**: store `DateTime` in UTC; use `ShopClock` (Vietnam UTC+7) for local dates and report grouping.

### Areas and authorization

- Customer storefront: root `Controllers/` + `Views/` (products, cart, orders/checkout, warranty claims, notifications).
- `Areas/Admin`: every controller is `[Authorize(Roles = "Admin")]` (products/variants/specs/stock, catalog groups, customers, staff accounts, orders, warranty policies & defect categories, reports with Excel export via the in-house `XlsxWriter`).
- `Areas/Staff`: `[Authorize(Roles = "Admin,Staff")]` (order processing, warranty claims, notifications).
- Order processing is shared: both areas' `OrdersController` are one-line subclasses of `Controllers/Management/OrderManagementController` (list with status tabs, one-click "next step", bulk `Advance`, `returnUrl` kept so staff land back on their filtered list), rendering `Views/Shared/_OrderManagementList` / `_OrderManagementDetails`; status texts live in `Models/OrderDisplay`. `wwwroot/js/admin.js` (both layouts) adds active sidebar link, `/` to focus search, auto-submitting filter selects, `data-confirm`, double-submit protection, select-all/bulk bar, `data-copy`. `Views/Shared/_StaffPermissions.cshtml` documents what Staff can do — keep it in sync with the `[Authorize]` attributes.
- `Areas/Identity`: scaffolded Identity Razor Pages, customized. Uses `AddIdentity` (not `AddDefaultIdentity`), so only pages present in the repo exist.
- Login redirects: `/Admin` and `/Staff` paths go to `/admin/login`, everything else to `/login` (see `ConfigureApplicationCookie` in `Program.cs`).

### Identity specifics

- Two kinds of accounts: customers registering with email (UserName = email, must confirm via mailed link) and staff/admin/legacy customers with no email (UserName = normalized phone number, see `PhoneNumberFormatter`). `EmailUserConfirmation` decides "confirmed" per account type; `RequireUniqueEmail` is deliberately off.
- `ForcePasswordChangeMiddleware` forces users whose password was set by an admin (new staff, reset) to change it before continuing.
- Security stamp revalidates every minute so locking an account (`AccountLockService`) signs the user out quickly.
- `VietnameseIdentityErrorDescriber` localizes Identity errors.

### Domain flows

- **Orders**: status progresses Pending → Confirmed → Processing → Shipping → Completed; cancellation allowed before Shipping, requires a reason, and restocks items (`InventoryTransaction` records stock movements). Payment: COD (marked paid on delivery) or VNPay (`VnPayPaymentService`, sandbox config under `Payment:VnPay` in user secrets). VNPay details: each payment attempt gets a new `vnp_TxnRef` = `{orderId}P{timestamp}`; the signed return URL is applied with the same idempotent handler as the IPN (`ProcessVnPayNotificationAsync`), so payment is recorded even on localhost where IPN can't arrive; unpaid VNPay orders stay `Pending` for `PaymentTimeoutMinutes` (default 30) and are then auto-cancelled with restock by the hosted `OnlinePaymentExpiryService`; customers can switch an unpaid order to COD; staff are notified of a VNPay order only once it is paid and cannot advance it before. Order codes come from `ShortCodeGenerator`. Shipping fee/free threshold from `ShippingOptions`.
- **Warranty claims**: customers file claims with video/image evidence (stored privately by `ClaimMediaStorage` in `App_Data/claims/{claimId}`, not publicly served; product images by `ProductImageStorage` in `wwwroot/uploads/products`, served by `UseStaticFiles`). Eligibility windows come from warranty policies per brand/category (`WarrantyPolicyService`). Staff move claims through review/receive/send-to-manufacturer/replace/complete.
- **Notifications**: persisted `Notification` rows + real-time push over SignalR (`Hubs/NotificationHub`, `/hubs/notifications`, push-only via `Clients.User(userId)`); client in `wwwroot/js/notifications.js`.

- **Racket advisor** (`/Advisor`, "Tư vấn chọn vợt"): `AdvisorController` → `RacketAdvisorService` → `AdvisorRepository`. A 4-question `PlayerProfile` (level, style, wrist, budget, optional brand) is scored against racket specs looked up by attribute name (`Điểm cân bằng`, `Độ cứng thân`, `Trọng lượng`; strings by `Đường kính`) — renaming those spec attributes breaks it. Fit = 35% balance + 30% flex + 15% weight + 20% price (+5% preferred brand), each part emitting a `FitReason`. The profile is stored in the `pp_player` cookie; `ProductsController.Details` uses it to show the fit panel (`Views/Shared/_RacketFit`) and preselect the suggested variant. Catalog reads are cached 2 min in `IMemoryCache`.

## Theme

- One brand theme for every layout: `wwwroot/css/theme.css` is loaded LAST in the storefront (`_MainStylePartial`), Identity, Admin and Staff layouts. It sets the Nunito font, orange tokens (`--brand` #ff6c00 …) and re-colours Bootstrap 4 (storefront) and Bootstrap 5 (back office, Identity) components. Use those tokens / `primary` classes instead of hard-coded colours; keep success/warning/danger semantic. The template's `main.css` (and `wwwroot/scss`) now uses Nunito too.

## Guidelines

- From `.github/copilot-instructions.md`: when revising an existing dashboard or UI, keep existing charts and established components unless explicitly asked to remove them; if a kept chart is a placeholder, wire it to real data instead of deleting it.
- `Doc/` is excluded from compilation; `Docs/` holds thesis documents, diagrams (.drawio), and test plans.
