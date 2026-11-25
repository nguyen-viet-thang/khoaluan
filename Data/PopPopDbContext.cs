using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;

namespace PopPop.Data
{
    public class PopPopDbContext : IdentityDbContext<User, Role, string>
    {
        public PopPopDbContext(DbContextOptions<PopPopDbContext> options) 
            : base(options)
        {
        }

        #region DbSets for Business Entities

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
        public DbSet<DefectType> DefectTypes { get; set; } = null!;
        public DbSet<WarrantyPolicy> WarrantyPolicies { get; set; } = null!;
        public DbSet<WarrantyClaim> WarrantyClaims { get; set; } = null!;
        public DbSet<WarrantyClaimMedia> WarrantyClaimMedia { get; set; } = null!;
        public DbSet<WarrantyClaimHistory> WarrantyClaimHistory { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<SpecAttribute> SpecAttributes { get; set; } = null!;
        public DbSet<SpecOption> SpecOptions { get; set; } = null!;
        public DbSet<ProductSpecValue> ProductSpecValues { get; set; } = null!;
        public DbSet<VariantSpecValue> VariantSpecValues { get; set; } = null!;

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region User Configuration

            modelBuilder.Entity<User>(entity =>
            {
                // Existing accounts get the migration time as their registration date.
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LockReason)
                    .HasMaxLength(500);
                // Email / NormalizedEmail / EmailConfirmed (+ EmailIndex) come from the Identity base mapping:
                // customers sign in with a verified email, staff/admin/older accounts with their phone number.
            });

            #endregion

            #region Category Configuration

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            #endregion

            #region Brand Configuration

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            #endregion

            #region Product Configuration

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region ProductVariant Configuration

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SKU)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.VariantLabel)
                    .HasMaxLength(100);
                entity.HasIndex(e => e.SKU)
                    .IsUnique();

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity)
                    .HasDefaultValue(0);

                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductVariants)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region ProductImage Configuration

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductImages)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Cart Configuration

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId)
                    .IsUnique();
            });

            #endregion

            #region CartItem Configuration

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice)
                    .HasPrecision(18, 2);
                entity.Property(e => e.Quantity)
                    .HasDefaultValue(1);

                entity.HasOne(e => e.Cart)
                    .WithMany(e => e.CartItems)
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductVariant)
                    .WithMany(e => e.CartItems)
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region Order Configuration

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId)
                    .IsRequired();
                entity.Property(e => e.OrderCode)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.OrderCode)
                    .IsUnique();

                entity.Property(e => e.PaymentMethod)
                    .HasDefaultValue(PaymentMethod.CashOnDelivery)
                    .HasSentinel((PaymentMethod)0);
                entity.Property(e => e.PaymentStatus)
                    .HasDefaultValue(PaymentStatus.Unpaid)
                    .HasSentinel((PaymentStatus)0);

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(18, 2);
                entity.Property(e => e.ShippingFee)
                    .HasPrecision(18, 2);
                entity.Property(e => e.RecipientName)
                    .HasMaxLength(256);
                entity.Property(e => e.RecipientPhone)
                    .HasMaxLength(20);
                entity.Property(e => e.ShippingAddress)
                    .HasMaxLength(500);
                entity.Property(e => e.Note)
                    .HasMaxLength(1000);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region OrderItem Configuration

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.SKU)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.UnitPrice)
                    .HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductVariant)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region InventoryTransaction Configuration

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Note)
                    .HasMaxLength(500);

                entity.HasOne(e => e.ProductVariant)
                    .WithMany(e => e.InventoryTransactions)
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Warranty / claims Configuration

            modelBuilder.Entity<DefectType>(entity =>
            {
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Recognition).HasMaxLength(500);
                entity.Property(e => e.EvidenceGuide).HasMaxLength(500);
                entity.Property(e => e.Source).HasMaxLength(256);
                entity.Property(e => e.SourceUrl).HasMaxLength(500);
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WarrantyPolicy>(entity =>
            {
                entity.HasIndex(e => new { e.BrandId, e.CategoryId }).IsUnique();
                entity.Property(e => e.CoveredParts).HasMaxLength(256);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.SourceUrl).HasMaxLength(500);
                entity.HasOne(e => e.Brand).WithMany().HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WarrantyClaim>(entity =>
            {
                entity.Property(e => e.Code).IsRequired().HasMaxLength(40);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.Status, e.CreatedAt });
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.StaffNote).HasMaxLength(2000);
                entity.Property(e => e.ReturnCarrier).HasMaxLength(100);
                entity.Property(e => e.ReturnTrackingCode).HasMaxLength(100);
                entity.Property(e => e.OutboundCarrier).HasMaxLength(100);
                entity.Property(e => e.OutboundTrackingCode).HasMaxLength(100);
                // Claims are legal/audit records: never removed together with orders, users or defect types.
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.OrderItem).WithMany().HasForeignKey(e => e.OrderItemId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DefectType).WithMany().HasForeignKey(e => e.DefectTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WarrantyClaimMedia>(entity =>
            {
                entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Claim).WithMany(e => e.Media).HasForeignKey(e => e.ClaimId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WarrantyClaimHistory>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(2000);
                entity.Property(e => e.ActorName).IsRequired().HasMaxLength(256);
                entity.HasOne(e => e.Claim).WithMany(e => e.History).HasForeignKey(e => e.ClaimId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.Url).HasMaxLength(300);
                entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SpecAttribute>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Hint).HasMaxLength(300);
                entity.HasIndex(e => new { e.CategoryId, e.Name }).IsUnique();
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SpecOption>(entity =>
            {
                entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Hint).HasMaxLength(300);
                entity.HasIndex(e => new { e.SpecAttributeId, e.Value }).IsUnique();
                entity.HasOne(e => e.Attribute).WithMany(e => e.Options).HasForeignKey(e => e.SpecAttributeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductSpecValue>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.SpecOptionId });
                entity.HasIndex(e => e.SpecOptionId);
                entity.HasOne(e => e.Product).WithMany(e => e.SpecValues).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
                // Restrict: an option still in use cannot be deleted silently.
                entity.HasOne(e => e.Option).WithMany().HasForeignKey(e => e.SpecOptionId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VariantSpecValue>(entity =>
            {
                entity.HasKey(e => new { e.ProductVariantId, e.SpecOptionId });
                entity.HasIndex(e => e.SpecOptionId);
                entity.HasOne(e => e.Variant).WithMany(e => e.SpecValues).HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Option).WithMany().HasForeignKey(e => e.SpecOptionId).OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region Seed Data

            SeedRoles(modelBuilder);
            Seed.CatalogSeedData.Apply(modelBuilder);
            Seed.WarrantySeedData.Apply(modelBuilder);
            Seed.SpecSeedData.Apply(modelBuilder);
            SeedAdmin(modelBuilder);
            Seed.UserSeedData.Apply(modelBuilder);

            #endregion
        }

        /// <summary>
        /// Seed default roles
        /// </summary>
        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Id = "1",
                    ConcurrencyStamp = "fbcad7b2-f545-4c63-8337-f7adacba6d58",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator with full system access"
                },
                new Role
                {
                    Id = "2",
                    ConcurrencyStamp = "481a46f1-206a-4e75-bed6-893a0569336f",
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    Description = "Staff member for shop operations"
                },
                new Role
                {
                    Id = "3",
                    ConcurrencyStamp = "ce2b0c57-c43d-45a5-8fa4-9fd44e0e1b9b",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    Description = "Regular customer"
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);
        }

        /// <summary>
        /// Seed admin user
        /// </summary>
        private static void SeedAdmin(ModelBuilder modelBuilder)
        {
            var adminUser = new User
            {
                Id = "admin-001",
                ConcurrencyStamp = "5336cfb7-e6b0-46a0-944a-8b862c73bc55",
                // Sign in at /admin/login with this phone number (password Admin@123, change it before going live).
                UserName = "0900000000",
                NormalizedUserName = "0900000000",
                PhoneNumber = "0900000000",
                FullName = "Admin PôpPôp",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = "e43821f8-8664-47f1-a77b-a2bda06ff0a6",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                PasswordHash = "AQAAAAIAAYagAAAAEOgDAa2IObWSVYVqlqcszoIHA5N2mQj3W6QRnKPnI7Hn28z5UkPjLktIhZRW4q3tBA=="
            };

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed admin role assignment
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUser.Id,
                    RoleId = "1"
                }
            );
        }
    }
}
