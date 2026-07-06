using Microsoft.AspNetCore.Mvc;

namespace FYP_InternshipManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Student"))
                return RedirectToAction("Listings", "Student");
            if (User.IsInRole("Company"))
                return RedirectToAction("Listings", "Company");
            if (User.IsInRole("Administrator"))
                return RedirectToAction("ManageAccount", "Admin");

            return View();
        }
    }
}
