using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;

namespace SignPuddle.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sign> Signs { get; set; } = default!;
        public DbSet<Symbol> Symbols { get; set; } = default!;
        public DbSet<Dictionary> Dictionaries { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and indexes
            modelBuilder.Entity<Sign>()
                .HasIndex(s => s.DictionaryId);

            modelBuilder.Entity<Sign>()
                .HasOne(s => s.Dictionary)
                .WithMany()
                .HasForeignKey(s => s.DictionaryId);
        }
    }
}