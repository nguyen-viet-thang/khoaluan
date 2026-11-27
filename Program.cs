using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Implementations;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Implementations;
using PopPop.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<PopPopDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Ignore PendingModelChangesWarning during development to allow model updates
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Customers register with an email (= user name) that must be verified via a mailed link before signing in.
// Staff/admin and older customer accounts have no email: their (normalised) phone number is the user name.
// AddIdentity (not AddDefaultIdentity) so only our own Identity pages exist.
builder.Services.AddIdentity<User, Role>(options =>
    {
        // "Confirmed" is decided by EmailUserConfirmation: email accounts need EmailConfirmed, phone accounts don't.
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = false;
        // Uniqueness comes from UserName = email; RequireUniqueEmail would reject the phone accounts (no email).
        options.User.RequireUniqueEmail = false;
        // Same key length AddDefaultIdentity used, so the existing login/token tables keep their schema.
        options.Stores.MaxLengthForKeys = 128;
    })
    .AddEntityFrameworkStores<PopPopDbContext>()
    .AddErrorDescriber<VietnameseIdentityErrorDescriber>()
    .AddDefaultTokenProviders()
    .AddUserConfirmation<EmailUserConfirmation>()
    .AddClaimsPrincipalFactory<PopPopClaimsPrincipalFactory>();
builder.Services.Configure<PopPop.Models.SmtpOptions>(builder.Configuration.GetSection(PopPop.Models.SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
});
builder.Services.AddScoped<IAccountPasswordService, AccountPasswordService>();
// Re-check the security stamp every minute so a locked account is signed out quickly
// (locking a customer updates their security stamp).
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(1));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Events.OnRedirectToLogin = context =>
    {
        var isManagementArea = context.Request.Path.StartsWithSegments("/Admin")
            || context.Request.Path.StartsWithSegments("/Staff");
        var loginPath = isManagementArea ? "/admin/login" : options.LoginPath.Value!;
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect(Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(loginPath, "ReturnUrl", returnUrl));
        return Task.CompletedTask;
    };
});
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// CORS configuration
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ---Shipping fee policy (defaults in ShippingOptions when the section is missing)---
builder.Services.Configure<PopPop.Models.ShippingOptions>(builder.Configuration.GetSection(PopPop.Models.ShippingOptions.SectionName));
// ---Public shop contact info for the footer (optional)---
builder.Services.Configure<PopPop.Models.StoreOptions>(builder.Configuration.GetSection(PopPop.Models.StoreOptions.SectionName));
builder.Services.Configure<PopPop.Models.VnPayOptions>(builder.Configuration.GetSection(PopPop.Models.VnPayOptions.SectionName));

// Register repositories and unit of work
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductManagementRepository, ProductManagementRepository>();
builder.Services.AddScoped(typeof(ICatalogGroupRepository<>), typeof(CatalogGroupRepository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerManagementService, CustomerManagementService>();
builder.Services.AddScoped<IAccountLockService, AccountLockService>();
builder.Services.AddScoped<IStaffAccountService, StaffAccountService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IWarrantyClaimRepository, WarrantyClaimRepository>();
builder.Services.AddScoped<IWarrantyPolicyRepository, WarrantyPolicyRepository>();
builder.Services.AddSingleton<IClaimMediaStorage, ClaimMediaStorage>();
builder.Services.AddScoped<IWarrantyClaimService, WarrantyClaimService>();
builder.Services.AddScoped<IWarrantyPolicyService, WarrantyPolicyService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISpecRepository, SpecRepository>();
builder.Services.AddScoped<ISpecManagementService, SpecManagementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdvisorRepository, AdvisorRepository>();
builder.Services.AddScoped<IRacketAdvisorService, RacketAdvisorService>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IHeaderBadgeService, HeaderBadgeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVnPayPaymentService, VnPayPaymentService>();
builder.Services.AddHostedService<OnlinePaymentExpiryService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProductManagementService, ProductManagementService>();
builder.Services.AddSingleton<IProductImageStorage, ProductImageStorage>();
builder.Services.AddScoped<ICatalogGroupService<Category>, CategoryManagementService>();
builder.Services.AddScoped<ICatalogGroupService<Brand>, BrandManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();
// MapStaticAssets only serves files known at build time; this serves images uploaded at runtime
// (wwwroot/uploads) and only runs when no endpoint matched the request.
app.UseStaticFiles();
app.UseCors("Frontend");

app.UseAuthentication();
// Accounts whose password was set by an admin (new staff, password reset) must choose their own first.
app.UseMiddleware<PopPop.Middleware.ForcePasswordChangeMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<PopPop.Hubs.NotificationHub>("/hubs/notifications");

app.Run();