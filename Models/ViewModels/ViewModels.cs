using FYP_InternshipManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace FYP_InternshipManagementSystem.Models.ViewModels
{
    // ─── Auth ───────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ChangeEmailViewModel
    {
        [Required, EmailAddress]
        public string NewEmail { get; set; } = string.Empty;

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
    }

    // ─── Student ────────────────────────────────────────────
    public class StudentProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^\d{10}$",
        ErrorMessage = "Contact number must contain exactly 10 digits.")]
        public string ContactNumber { get; set; } = string.Empty;
        public string EducationalInstitution { get; set; } = string.Empty;
        public string Programme { get; set; } = string.Empty;
        public decimal CGPA { get; set; }

        // Existing stored paths — passed back as hidden fields so they survive round-trip
        public string? ProfilePicPath { get; set; }
        public string? ResumeName { get; set; }
        public string? ResumePath { get; set; }

        // New uploads — always optional
        public IFormFile? ProfilePicFile { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }

    public class ApplyInternshipViewModel
    {
        public int InternshipId { get; set; }

        // These are populated by the controller for display only — never bound from form data
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public Internship Internship { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public Student Student { get; set; } = null!;

        public string? CoverLetter { get; set; }
        public IFormFile? ResumeFile { get; set; }
        public List<IFormFile>? SupportingDocuments { get; set; }
    }

    public class InternshipListViewModel
    {
        public List<Internship> Internships { get; set; } = new();
        public List<int> SavedInternshipIds { get; set; } = new();
        public List<int> AppliedInternshipIds { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? Location { get; set; }
        public string? Department { get; set; }
    }

    // ─── Company ────────────────────────────────────────────
    public class CompanyProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyContactNum { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompanyLocation { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public string? ProfilePicPath { get; set; }
        public IFormFile? ProfilePicFile { get; set; }
    }

    public class PostInternshipViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Department { get; set; } = string.Empty;
        [Required]
        public string Duration { get; set; } = string.Empty;
        [Required]
        public string Allowance { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
    }

    public class EditInternshipViewModel
    {
        public int InternshipId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Department { get; set; } = string.Empty;
        [Required]
        public string Duration { get; set; } = string.Empty;
        [Required]
        public string Allowance { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CompanyDashboardViewModel
    {
        public int ActiveListingCount { get; set; }
        public int TotalApplicantCount { get; set; }
        public int PendingReviewCount { get; set; }
        public List<Internship> Internships { get; set; } = new();
    }
}

// ─── Admin ──────────────────────────────────────────
public class AdminManageAccountViewModel
{
    public List<Student> Students { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public int TotalStudents { get; set; }
    public int TotalCompanies { get; set; }
    public int ActiveAccounts { get; set; }
    public int DeactivatedAccounts { get; set; }
    public string? StatusFilter { get; set; }
}

public class AdminApproveUsersViewModel
{
    public List<ApplicationUser> PendingUsers { get; set; } = new();
    public List<ApplicationUser> DeactivatedUsers { get; set; } = new();
    public int PendingCount { get; set; }
    public int ApprovedToday { get; set; }
    public int RejectedCount { get; set; }
    public int DeactivatedCount { get; set; }
}

public class AdminInternshipRecordsViewModel
{
    public List<Internship> Internships { get; set; } = new();
    public string? StatusFilter { get; set; }
}

public class AdminApplicationsViewModel
{
    public List<Application> Applications { get; set; } = new();
    public int TotalApplications { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public string? StatusFilter { get; set; }
}

public class AdminReportsViewModel
{
    public int TotalStudents { get; set; }
    public int ActiveInternships { get; set; }
    public int CompletedApplications { get; set; }
    public double PlacementRate { get; set; }
    public List<ProgrammePlacementRow> PlacementByProgramme { get; set; } = new();
}

public class ProgrammePlacementRow
{
    public string Programme { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int Placed { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public string Rate => TotalStudents > 0 ? $"{(Placed * 100.0 / TotalStudents):F1}%" : "0%";
}

public class AdminEditStudentViewModel
{
    public int StudentId { get; set; }

    public string? ProfilePicture { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Programme { get; set; } = string.Empty;
    public string EducationalInstitution { get; set; } = string.Empty;
    public decimal CGPA { get; set; }
    public string Status { get; set; } = "Active";
}

public class AdminEditCompanyViewModel
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyContactNum { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyLocation { get; set; } = string.Empty;
    public string IndustryType { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

