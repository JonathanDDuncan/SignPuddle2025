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
        public DbSet<Dictionary> Dictionaries { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Sign primary key
            modelBuilder.Entity<Sign>()
                .HasKey(s => s.Id);

            // Configure relationships and indexes
            modelBuilder.Entity<Sign>()
                .ToContainer("Signs")
                .HasPartitionKey(s => s.DictionaryId)
                .HasNoDiscriminator();

            modelBuilder.Entity<Sign>()
                .HasOne(s => s.Dictionary)
                .WithMany()
                .HasForeignKey(s => s.DictionaryId);          
     


            // Configure Metadata as a owned property to resolve navigation issues    
            // No need to ignore MetadataJson as it's a regular string property
 
            // Configure Dictionary container
            modelBuilder.Entity<Dictionary>()
                .ToContainer("Dictionaries")
                .HasNoDiscriminator();

            // Configure User container
            modelBuilder.Entity<User>()
                .ToContainer("Users")
                .HasNoDiscriminator();
        }
    }
}