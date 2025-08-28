using System.Data.Entity;
using POSLibrary.Entities;

namespace ProductLibrary.Data
{
    public class POSDbContext : DbContext
    {
        // Constructor -> Connection string name from App.config / Web.config
        public POSDbContext() : base("name=POSDbConnection")
        {
            // Optional: Disable Lazy Loading (makes queries more predictable)
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        // DbSets (Tables)
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        // Optional: Model customizations
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table Names (Optional – otherwise EF uses plural form)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<SaleItem>().ToTable("SaleItems");

            // Relationships
            modelBuilder.Entity<Sale>()
                        .HasMany(s => s.SaleItems)
                        .WithRequired(si => si.Sale)
                        .HasForeignKey(si => si.SaleId);
        }
    }
}
