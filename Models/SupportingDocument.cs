using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int SupportingDocId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        public string SupportingDocName { get; set; } = string.Empty;
        public string SupportingDocPath { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; } = null!;
    }
}
