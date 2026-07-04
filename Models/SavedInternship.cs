using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class SavedInternship
    {
        [Key]
        public int SavedInternshipId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int InternshipId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [ForeignKey("InternshipId")]
        public Internship Internship { get; set; } = null!;
    }
}
