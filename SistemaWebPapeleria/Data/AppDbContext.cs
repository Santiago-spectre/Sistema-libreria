using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Definición de las 8 tablas
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<CashClosing> CashClosings { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //1. Role -> User (1:M)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // 1. Category -> Product (1:M)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // 2. Supplier -> Product (1:M), opcional
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .IsRequired(false);

            // 3. Product -> SaleDetail (1:M)
            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Product)
                .WithMany(p => p.SaleDetails)
                .HasForeignKey(sd => sd.ProductId);

            // 4. User -> Sale (1:M)
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.UserId);

            // 5. Sale -> SaleDetail (1:M)
            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId);

            // 6. Sale -> Receipt (1:1)
            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.Sale)
                .WithOne(s => s.Receipt)
                .HasForeignKey<Receipt>(r => r.SaleId);

            // 7. User -> CashClosing (1:M)
            modelBuilder.Entity<CashClosing>()
                .HasOne(cc => cc.User)
                .WithMany(u => u.CashClosings)
                .HasForeignKey(cc => cc.UserId);

            // Configuración de precisión para campos decimales
            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.Discount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SaleDetail>()
                .Property(sd => sd.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SaleDetail>()
                .Property(sd => sd.Subtotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CashClosing>()
                .Property(cc => cc.InitialAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CashClosing>()
                .Property(cc => cc.TotalSales)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CashClosing>()
                .Property(cc => cc.ClosingAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Administrador" },
                new Role { RoleId = 2, RoleName = "Vendedor" }
                );
        }
    }
}
