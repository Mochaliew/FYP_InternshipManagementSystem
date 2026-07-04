using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string CompanyContactNum { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyLocation { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Internship> Internships { get; set; } = new List<Internship>();
    }
}
