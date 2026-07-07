using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public string EducationalInstitution { get; set; } = string.Empty;
        public string Programme { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3,2)")]
        public decimal CGPA { get; set; }

        public string? ProfilePic { get; set; }
        public string? ResumePath { get; set; }
        public string? ResumeName { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<SavedInternship> SavedInternships { get; set; } = new List<SavedInternship>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
