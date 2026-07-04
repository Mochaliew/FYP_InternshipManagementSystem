using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_InternshipManagementSystem.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db; _userManager = userManager; _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (!result.Succeeded) { ModelState.AddModelError("", "Invalid credentials."); return View(model); }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalStudents = await _db.Students.CountAsync();
            ViewBag.TotalCompanies = await _db.Companies.CountAsync();
            ViewBag.TotalInternships = await _db.Internships.CountAsync();
            ViewBag.TotalApplications = await _db.Applications.CountAsync();
            ViewBag.PendingApplications = await _db.Applications.CountAsync(a => a.Status == "Pending");
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Students()
        {
            var students = await _db.Students.Include(s => s.User).ToListAsync();
            return View(students);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Companies()
        {
            var companies = await _db.Companies.Include(c => c.User).ToListAsync();
            return View(companies);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Internships()
        {
            var internships = await _db.Internships
                .Include(i => i.Company).ThenInclude(c => c.User)
                .Include(i => i.Applications)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return View(internships);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Applications()
        {
            var applications = await _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            return View(applications);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
