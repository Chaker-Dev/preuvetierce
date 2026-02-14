using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PreuveTierce.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Exemples de champs supplémentaires
        // public string FullName { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Proof> Proofs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Proof>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.DocumentHash).IsRequired();
                entity.Property(p => p.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.CertificateSerial).IsRequired(false);
            });
        }
    }
    public class Proof
    {
        public int Id { get; set; }
        public string DocumentHash { get; set; } = null!;
        public string? Reference { get; set; }
        public DateTime Timestamp { get; set; }
        public string? CertificateSerial { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
