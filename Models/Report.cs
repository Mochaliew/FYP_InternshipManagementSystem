using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int AdminId { get; set; }

        public DateTime GenerateDate { get; set; } = DateTime.Now;
        public string ReportName { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;

        [ForeignKey("AdminId")]
        public Administrator Administrator { get; set; } = null!;
    }
}
