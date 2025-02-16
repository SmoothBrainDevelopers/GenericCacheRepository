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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}
