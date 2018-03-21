using Microsoft.EntityFrameworkCore;

namespace WhoCodes.Models
{
    public class WhoCodesDbContext : DbContext
    {
        public WhoCodesDbContext(DbContextOptions<WhoCodesDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .ToTable("Companies");

            modelBuilder.Entity<Contact>()
                .ToTable("Contacts")
                .HasOne(e => e.Company)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContactSkill>()
                .ToTable("ContactSkills")
                .HasKey(e => new { e.ContactId, e.SkillId });

            modelBuilder.Entity<ContactSkill>()
                .HasOne(e => e.Contact)
                .WithMany(e => e.Skills)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactSkill>()
                .HasOne(e => e.Skill)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Skill>()
                .ToTable("Skills");
        }
    }

}
