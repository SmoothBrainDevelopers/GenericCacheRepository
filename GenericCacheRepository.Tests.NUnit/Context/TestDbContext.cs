using GenericCacheRepository.Tests.NUnit.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GenericCacheRepository.Tests.NUnit.Context
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() { }
        public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.RegionId);
                entity.HasMany(e => e.Stores)
                      .WithOne()
                      .HasForeignKey(e => e.StoreId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.StoreId);
                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(e => e.Region)
                      .WithMany(r => r.Stores)
                      .HasForeignKey(e => e.RegionId);
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.SaleId);

                entity.HasOne(e => e.Store)
                      .WithMany(s => s.Sales)
                      .HasForeignKey(e => e.StoreId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.DiscountBudgetUsed)
                      .HasConversion<double>()  // Fix for SQLite
                      .IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Price)
                      .HasConversion<double>()  // Fix for SQLite
                      .IsRequired();
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => new { e.CustomerId, e.StoreId, e.ProductId, e.PurchaseDate });

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Purchases)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Store)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(e => e.StoreId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}
