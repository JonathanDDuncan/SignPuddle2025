using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;

namespace SignPuddle.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }        public DbSet<Sign> Signs { get; set; } = default!;
        public DbSet<Symbol> Symbols { get; set; } = default!;
        public DbSet<Dictionary> Dictionaries { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<SpmlDocumentEntity> SpmlDocuments { get; set; } = default!;        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and indexes
            modelBuilder.Entity<Sign>()
                .HasIndex(s => s.DictionaryId);

            modelBuilder.Entity<Sign>()
                .HasOne(s => s.Dictionary)
                .WithMany()
                .HasForeignKey(s => s.DictionaryId);            // Configure SPML document entity for CosmosDB
            modelBuilder.Entity<SpmlDocumentEntity>()
                .ToContainer("SpmlDocuments")
                .HasPartitionKey(e => e.PartitionKey)
                .HasNoDiscriminator();
                
            // Configure SpmlDocument as an owned entity
            modelBuilder.Entity<SpmlDocumentEntity>()
                .OwnsOne(e => e.SpmlDocument);

            // Ignore SpmlDocument as a standalone entity
            modelBuilder.Ignore<SpmlDocument>();

            // Configure Metadata as a owned property to resolve navigation issues            // No need to ignore MetadataJson as it's a regular string property

            // Add indexes for common queries
            modelBuilder.Entity<SpmlDocumentEntity>()
                .Property(e => e.DocumentType);

            modelBuilder.Entity<SpmlDocumentEntity>()
                .Property(e => e.OwnerId);
        }
    }
}