using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;
    }
}
