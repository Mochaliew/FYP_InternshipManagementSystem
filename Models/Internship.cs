using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Internship
    {
        [Key]
        public int InternshipId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Allowance { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey("CompanyId")]
        public Company Company { get; set; } = null!;
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<SavedInternship> SavedInternships { get; set; } = new List<SavedInternship>();
    }
}
