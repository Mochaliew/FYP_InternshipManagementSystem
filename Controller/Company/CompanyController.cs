using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_InternshipManagementSystem.Controllers.Company
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CompanyController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _db = db; _userManager = userManager; _env = env;
        }

        private async Task<FYP_InternshipManagementSystem.Models.Company> GetCompanyAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _db.Companies.Include(c => c.User)
                .FirstAsync(c => c.UserId == user!.Id);
        }

        private void SetViewBag(FYP_InternshipManagementSystem.Models.Company company)
        {
            ViewBag.CompanyName = company.User.Name;
            ViewBag.ProfilePic = company.ProfilePic;
        }

        // ── Listings (Dashboard) ─────────────────────────────────
        public async Task<IActionResult> Listings()
        {
            var company = await GetCompanyAsync();
            var internships = await _db.Internships
                .Include(i => i.Applications)
                .Where(i => i.CompanyId == company.CompanyId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var vm = new CompanyDashboardViewModel
            {
                Internships = internships,
                ActiveListingCount = internships.Count(i => i.IsActive),
                TotalApplicantCount = internships.Sum(i => i.Applications.Count),
                PendingReviewCount = internships.Sum(i => i.Applications.Count(a => a.Status == "Pending"))
            };
            SetViewBag(company);
            return View(vm);
        }

        // ── Post Internship ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PostInternship()
        {
            var company = await GetCompanyAsync();
            SetViewBag(company);
            return View(new PostInternshipViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> PostInternship(PostInternshipViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var company = await GetCompanyAsync();

            _db.Internships.Add(new Internship
            {
                CompanyId = company.CompanyId,
                Title = model.Title,
                Department = model.Department,
                Duration = model.Duration,
                Allowance = model.Allowance,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                IsActive = true
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Internship posted successfully!";
            return RedirectToAction("Listings");
        }

        // ── Edit Internship ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditListing(int id)
        {
            var company = await GetCompanyAsync();
            var internship = await _db.Internships
                .FirstOrDefaultAsync(i => i.InternshipId == id && i.CompanyId == company.CompanyId);
            if (internship == null) return NotFound();

            SetViewBag(company);
            return View(new EditInternshipViewModel
            {
                InternshipId = internship.InternshipId,
                Title = internship.Title,
                Department = internship.Department,
                Duration = internship.Duration,
                Allowance = internship.Allowance,
                Description = internship.Description,
                IsActive = internship.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditListing(EditInternshipViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var company = await GetCompanyAsync();
            var internship = await _db.Internships
                .FirstOrDefaultAsync(i => i.InternshipId == model.InternshipId && i.CompanyId == company.CompanyId);
            if (internship == null) return NotFound();

            internship.Title = model.Title;
            internship.Department = model.Department;
            internship.Duration = model.Duration;
            internship.Allowance = model.Allowance;
            internship.Description = model.Description;
            internship.IsActive = model.IsActive;

            _db.Internships.Update(internship);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Listing updated successfully!";
            return RedirectToAction("Listings");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateListing(int id)
        {
            var company = await GetCompanyAsync();
            var internship = await _db.Internships
                .FirstOrDefaultAsync(i => i.InternshipId == id && i.CompanyId == company.CompanyId);
            if (internship != null)
            {
                internship.IsActive = false;
                _db.Internships.Update(internship);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Listings");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveListing(int id)
        {
            var company = await GetCompanyAsync();
            var internship = await _db.Internships
                .FirstOrDefaultAsync(i => i.InternshipId == id && i.CompanyId == company.CompanyId);
            if (internship != null)
            {
                _db.Internships.Remove(internship);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Listings");
        }

        // ── Applications ─────────────────────────────────────────
        public async Task<IActionResult> Applications()
        {
            var company = await GetCompanyAsync();
            var applications = await _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship)
                .Include(a => a.SupportingDocuments)
                .Where(a => a.Internship.CompanyId == company.CompanyId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            SetViewBag(company);
            return View(applications);
        }

        public async Task<IActionResult> ApplicationDetail(int id)
        {
            var company = await GetCompanyAsync();
            var application = await _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship)
                .Include(a => a.SupportingDocuments)
                .FirstOrDefaultAsync(a => a.ApplicationId == id && a.Internship.CompanyId == company.CompanyId);

            if (application == null) return NotFound();
            SetViewBag(company);
            return View(application);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, string status, string returnTo = "Applications")
        {
            var company = await GetCompanyAsync();
            var application = await _db.Applications
                .Include(a => a.Internship)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Internship.CompanyId == company.CompanyId);

            if (application != null)
            {
                application.Status = status;
                _db.Applications.Update(application);
                await _db.SaveChangesAsync();
            }

            if (returnTo == "Detail")
            {
                if (status == "Approved") TempData["ShowApprovedModal"] = "true";
                if (status == "Rejected") TempData["ShowRejectedBanner"] = "true";
                return RedirectToAction("ApplicationDetail", new { id = applicationId });
            }

            if (returnTo == "ApplicantProfile")
            {
                if (status == "Approved") TempData["ShowApprovedModal"] = "true";
                if (status == "Rejected") TempData["ShowRejectedBanner"] = "true";
                return RedirectToAction("ApplicantProfile", new { applicationId = applicationId });
            }

            if (status == "Approved") TempData["ShowApprovedModal"] = "true";
            return RedirectToAction("Applications");
        }

        public async Task<IActionResult> ApplicantProfile(int applicationId)
        {
            var company = await GetCompanyAsync();
            var application = await _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Internship.CompanyId == company.CompanyId);

            if (application == null) return NotFound();
            SetViewBag(company);

            if (TempData["ShowApprovedModal"] != null)
                ViewBag.ShowApprovedModal = true;
            if (TempData["ShowRejectedBanner"] != null)
                ViewBag.ShowRejectedBanner = true;

            return View(application);
        }

        // ── Profile ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var company = await GetCompanyAsync();
            var vm = new CompanyProfileViewModel
            {
                Name = company.User.Name,
                Email = company.User.Email!,
                CompanyContactNum = company.CompanyContactNum,
                Description = company.Description,
                CompanyLocation = company.CompanyLocation,
                IndustryType = company.IndustryType,
                ProfilePicPath = company.ProfilePic
            };
            SetViewBag(company);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(CompanyProfileViewModel model)
        {
            ModelState.Remove("ProfilePicFile");

            var company = await GetCompanyAsync();

            if (!ModelState.IsValid)
            {
                model.ProfilePicPath = company.ProfilePic;
                model.Email = company.User.Email!;
                SetViewBag(company);
                return View(model);
            }

            company.User.Name = model.Name;
            await _userManager.UpdateAsync(company.User);

            company.CompanyContactNum = model.CompanyContactNum;
            company.Description = model.Description;
            company.CompanyLocation = model.CompanyLocation;
            company.IndustryType = model.IndustryType;

            if (model.ProfilePicFile != null && model.ProfilePicFile.Length > 0)
            {
                try
                {
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(dir);
                    var fileName = $"company_{company.CompanyId}_{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicFile.FileName)}";
                    var fullPath = Path.Combine(dir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ProfilePicFile.CopyToAsync(stream);
                    }
                    company.ProfilePic = $"/uploads/profiles/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Logo upload failed: {ex.Message}";
                }
            }

            _db.Companies.Update(company);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user!, model.CurrentPassword, model.NewPassword);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password changed successfully!"
                : string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a valid email and your current password.";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);

            var passwordValid = await _userManager.CheckPasswordAsync(user!, model.CurrentPassword);
            if (!passwordValid)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            var existing = await _userManager.FindByEmailAsync(model.NewEmail);
            if (existing != null && existing.Id != user!.Id)
            {
                TempData["Error"] = "This email is already in use by another account.";
                return RedirectToAction("Profile");
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user!, model.NewEmail);
            var emailResult = await _userManager.ChangeEmailAsync(user!, model.NewEmail, token);
            if (!emailResult.Succeeded)
            {
                TempData["Error"] = string.Join(", ", emailResult.Errors.Select(e => e.Description));
                return RedirectToAction("Profile");
            }

            var usernameResult = await _userManager.SetUserNameAsync(user!, model.NewEmail);
            if (!usernameResult.Succeeded)
            {
                TempData["Error"] = string.Join(", ", usernameResult.Errors.Select(e => e.Description));
                return RedirectToAction("Profile");
            }

            var signInManager = HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            await signInManager.RefreshSignInAsync(user!);

            TempData["Success"] = "Email updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var signInManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.SignInManager<ApplicationUser>>();
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
