using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FYP_InternshipManagementSystem.Controllers.Company
{
    public class CompanyAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public CompanyAuthController(UserManager<ApplicationUser> u,
            SignInManager<ApplicationUser> s, ApplicationDbContext db)
        {
            _userManager = u; _signInManager = s; _db = db;
        }

        [HttpGet]
        public IActionResult Register() => View();






        // Register new company 

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Email.Split('@')[0],
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Company");
            _db.Companies.Add(new FYP_InternshipManagementSystem.Models.Company { UserId = user.Id });
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Listings", "Company");
        }






        // Login and Logout company

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Company"))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }
            return RedirectToAction("Listings", "Company");
        }





        // Forget Password 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all fields correctly.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.IsInRoleAsync(user, "Company"))
            {
                TempData["Error"] = "Company account not found.";
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.NewPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }

            TempData["Success"] = "Password reset successfully. Please login using your new password.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
