using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FYP_InternshipManagementSystem.Controllers.Student
{
    public class StudentAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public StudentAuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }





        // Register Student

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Email.Split('@')[0],
                EmailConfirmed = true,
                Status = "Active"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Student");
            _db.Students.Add(new FYP_InternshipManagementSystem.Models.Student
            {
                UserId = user.Id,
                EducationalInstitution = "Tunku Abdul Rahman University of Management and Technology"
            });
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Listings", "Student");
        }





        // Login and Logout Student

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Check password first, then show account-status popup.
            // This is important because PasswordSignInAsync may only return "not succeeded"
            // when the account is locked/deactivated, so the custom popup will not appear.
            var passwordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordCorrect)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if (user.Status == "Deactivated")
            {
                ViewBag.PopupMessage = "Your student account has been deactivated. Please contact the administrator for assistance.";
                return View(model);
            }

            if (user.Status == "Pending")
            {
                ViewBag.PopupMessage = "Your student account is still pending approval. Please wait for administrator approval.";
                return View(model);
            }

            if (user.Status == "Rejected")
            {
                ViewBag.PopupMessage = "Your student account registration has been rejected. Please contact the administrator for assistance.";
                return View(model);
            }

            // If the account was previously locked by admin, prevent login unless status is Active.
            if (user.Status != "Active")
            {
                ViewBag.PopupMessage = "Your student account is not active. Please contact the administrator for assistance.";
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
            return RedirectToAction("Listings", "Student");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
