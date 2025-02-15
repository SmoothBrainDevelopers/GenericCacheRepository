using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericCacheRepository.Test.MS
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() { }
        public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }

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
