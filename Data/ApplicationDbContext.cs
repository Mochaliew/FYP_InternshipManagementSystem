using FYP_InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FYP_InternshipManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Internship> Internships { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<SavedInternship> SavedInternships { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser -> Student (1-to-1)
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser -> Company (1-to-1)
            builder.Entity<Company>()
                .HasOne(c => c.User)
                .WithOne(u => u.Company)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser -> Administrator (1-to-1)
            builder.Entity<Administrator>()
                .HasOne(a => a.User)
                .WithOne(u => u.Administrator)
                .HasForeignKey<Administrator>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company -> Internship (1-to-many)
            builder.Entity<Internship>()
                .HasOne(i => i.Company)
                .WithMany(c => c.Internships)
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student -> Application (1-to-many)
            builder.Entity<Application>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Internship -> Application (1-to-many)
            builder.Entity<Application>()
                .HasOne(a => a.Internship)
                .WithMany(i => i.Applications)
                .HasForeignKey(a => a.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            // Application -> SupportingDocument (1-to-many)
            builder.Entity<SupportingDocument>()
                .HasOne(sd => sd.Application)
                .WithMany(a => a.SupportingDocuments)
                .HasForeignKey(sd => sd.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student -> Document (1-to-many)
            builder.Entity<Document>()
                .HasOne(d => d.Student)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student -> SavedInternship (1-to-many)
            builder.Entity<SavedInternship>()
                .HasOne(si => si.Student)
                .WithMany(s => s.SavedInternships)
                .HasForeignKey(si => si.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Internship -> SavedInternship (1-to-many)
            builder.Entity<SavedInternship>()
                .HasOne(si => si.Internship)
                .WithMany(i => i.SavedInternships)
                .HasForeignKey(si => si.InternshipId)
                .OnDelete(DeleteBehavior.Restrict);

            // Administrator -> Report (1-to-many)
            builder.Entity<Report>()
                .HasOne(r => r.Administrator)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique indexes
            builder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            builder.Entity<Company>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            builder.Entity<Administrator>()
                .HasIndex(a => a.UserId)
                .IsUnique();

            // Prevent duplicate saved internship
            builder.Entity<SavedInternship>()
                .HasIndex(si => new { si.StudentId, si.InternshipId })
                .IsUnique();

            // CGPA precision
            builder.Entity<Student>()
                .Property(s => s.CGPA)
                .HasColumnType("decimal(3,2)");
        }
    }
}
