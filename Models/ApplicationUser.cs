using Microsoft.AspNetCore.Identity;

namespace FYP_InternshipManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
        // Role stored in Identity

        // Navigation
        public Student? Student { get; set; }
        public Company? Company { get; set; }
        public Administrator? Administrator { get; set; }
    }
}
