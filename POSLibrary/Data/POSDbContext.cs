using System.Data.Entity;
using POSLibrary.Entities;

namespace POSLibrary.Data
{
    public class POSDbContext : DbContext
    {
        // Connection string from App.config
        public POSDbContext() : base("name=POSDbConnection")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entities to tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<SaleItem>().ToTable("SaleItems");

            // Sale → SaleItems (One-to-Many)
            modelBuilder.Entity<Sale>()
                        .HasMany(s => s.SaleItems)
                        .WithRequired(si => si.Sale)
                        .HasForeignKey(si => si.SaleId);
        }
    }
}
