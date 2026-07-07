using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_InternshipManagementSystem.Models
{
    public class Administrator
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
