using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int InternshipId { get; set; }

        public string? CoverLetter { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        // "Pending", "Approved", "Rejected"
        public string Status { get; set; } = "Pending";

        public string? ResumePathSnapshot { get; set; }
        public string? ResumeNameSnapshot { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [ForeignKey("InternshipId")]
        public Internship Internship { get; set; } = null!;

        public ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
    }
}
