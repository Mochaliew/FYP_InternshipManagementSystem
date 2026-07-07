using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FYP_InternshipManagementSystem.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }




        // Dashboard Action

        public IActionResult Dashboard()
        {
            return RedirectToAction("ManageAccount");
        }





        // Login and Logout Actions

        [AllowAnonymous, HttpGet]
        public IActionResult Login() => View();

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            return RedirectToAction("ManageAccount");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }





        // Account Management Actions

        public async Task<IActionResult> ManageAccount(string? statusFilter)
        {
            var studentsQ = _db.Students.Include(s => s.User).AsQueryable();
            var companiesQ = _db.Companies.Include(c => c.User).AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                studentsQ = studentsQ.Where(s => s.User.Status == statusFilter);
                companiesQ = companiesQ.Where(c => c.User.Status == statusFilter);
            }

            var allUsers = await _db.Users.ToListAsync();

            var vm = new AdminManageAccountViewModel
            {
                Students = await studentsQ.ToListAsync(),
                Companies = await companiesQ.ToListAsync(),
                TotalStudents = await _db.Students.CountAsync(),
                TotalCompanies = await _db.Companies.CountAsync(),
                ActiveAccounts = allUsers.Count(u => u.Status == "Active"),
                DeactivatedAccounts = allUsers.Count(u => u.Status == "Deactivated"),
                StatusFilter = statusFilter
            };

            return View(vm);
        }





        // Edit Student and Company Actions

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null) return NotFound();

            return View(new AdminEditStudentViewModel
            {
                StudentId = student.StudentId,
                ProfilePicture = student.ProfilePic,
                Name = student.User.Name,
                Email = student.User.Email!,
                ContactNumber = student.ContactNumber,
                Programme = student.Programme,
                EducationalInstitution = student.EducationalInstitution,
                CGPA = student.CGPA,
                Status = student.User.Status
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(AdminEditStudentViewModel model)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

            if (student == null) return NotFound();

            student.User.Name = model.Name;
            student.User.Status = model.Status;
            student.ContactNumber = model.ContactNumber;
            student.Programme = model.Programme;
            student.EducationalInstitution = model.EducationalInstitution;
            student.CGPA = model.CGPA;

            await _userManager.UpdateAsync(student.User);
            _db.Students.Update(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Student updated.";
            return RedirectToAction("ManageAccount");
        }

        [HttpGet]
        public async Task<IActionResult> EditCompany(int id)
        {
            var company = await _db.Companies
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null) return NotFound();

            return View(new AdminEditCompanyViewModel
            {
                CompanyId = company.CompanyId,
                Name = company.User.Name,
                Email = company.User.Email!,
                CompanyContactNum = company.CompanyContactNum,
                Description = company.Description,
                CompanyLocation = company.CompanyLocation,
                IndustryType = company.IndustryType,
                Status = company.User.Status
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditCompany(AdminEditCompanyViewModel model)
        {
            var company = await _db.Companies
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CompanyId == model.CompanyId);

            if (company == null) return NotFound();

            company.User.Name = model.Name;
            company.User.Status = model.Status;
            company.CompanyContactNum = model.CompanyContactNum;
            company.Description = model.Description;
            company.CompanyLocation = model.CompanyLocation;
            company.IndustryType = model.IndustryType;

            await _userManager.UpdateAsync(company.User);
            _db.Companies.Update(company);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Company updated.";
            return RedirectToAction("ManageAccount");
        }






        // User Status Management Actions

        [HttpPost]
        public async Task<IActionResult> SetUserStatus(
            string userId,
            string status,
            string returnPage = "ManageAccount")
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.Status = status;

                await _userManager.SetLockoutEnabledAsync(user, status == "Deactivated");

                if (status == "Deactivated")
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                else
                    await _userManager.SetLockoutEndDateAsync(user, null);

                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"Account {status.ToLower()} successfully.";
            }

            return RedirectToAction(returnPage);
        }






        // Add Student and Company Actions

        [HttpGet]
        public IActionResult AddStudent() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> AddStudent(RegisterViewModel model)
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
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            _db.Students.Add(new FYP_InternshipManagementSystem.Models.Student
            {
                UserId = user.Id,
                EducationalInstitution = "Tunku Abdul Rahman University of Management and Technology"
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Student account created.";
            return RedirectToAction("ManageAccount");
        }

        [HttpGet]
        public IActionResult AddCompany() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> AddCompany(RegisterViewModel model)
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
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Company");

            _db.Companies.Add(new FYP_InternshipManagementSystem.Models.Company
            {
                UserId = user.Id
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Company account created.";
            return RedirectToAction("ManageAccount");
        }






        // Admin Approval Actions

        public async Task<IActionResult> ApproveUsers()
        {
            var allUsers = await _db.Users.ToListAsync();
            var today = DateTime.Today;

            var userRoles = new Dictionary<string, string>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userRoles[u.Id] = roles.FirstOrDefault() ?? "—";
            }

            ViewBag.UserRoles = userRoles;

            var vm = new AdminApproveUsersViewModel
            {
                PendingUsers = allUsers.Where(u => u.Status == "Pending").ToList(),
                DeactivatedUsers = allUsers.Where(u => u.Status == "Deactivated").ToList(),
                PendingCount = allUsers.Count(u => u.Status == "Pending"),
                ApprovedToday = allUsers.Count(u => u.Status == "Active" && u.CreatedAt.Date == today),
                RejectedCount = allUsers.Count(u => u.Status == "Rejected"),
                DeactivatedCount = allUsers.Count(u => u.Status == "Deactivated")
            };

            return View(vm);
        }





        // Internship and Application Management Actions

        public async Task<IActionResult> InternshipRecords(string? statusFilter)
        {
            var query = _db.Internships
                .Include(i => i.Company).ThenInclude(c => c.User)
                .Include(i => i.Applications)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool active = statusFilter == "Active";
                query = query.Where(i => i.IsActive == active);
            }

            var vm = new AdminInternshipRecordsViewModel
            {
                Internships = await query.OrderByDescending(i => i.CreatedAt).ToListAsync(),
                StatusFilter = statusFilter
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleInternshipStatus(int id)
        {
            var internship = await _db.Internships.FindAsync(id);

            if (internship != null)
            {
                internship.IsActive = !internship.IsActive;
                _db.Internships.Update(internship);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("InternshipRecords");
        }

        public async Task<IActionResult> Applications(string? statusFilter)
        {
            var query = _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .Include(a => a.SupportingDocuments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(a => a.Status == statusFilter);

            var all = await _db.Applications.ToListAsync();

            var vm = new AdminApplicationsViewModel
            {
                Applications = await query.OrderByDescending(a => a.AppliedAt).ToListAsync(),
                TotalApplications = all.Count,
                PendingCount = all.Count(a => a.Status == "Pending"),
                ApprovedCount = all.Count(a => a.Status == "Approved"),
                RejectedCount = all.Count(a => a.Status == "Rejected"),
                StatusFilter = statusFilter
            };

            return View(vm);
        }






        // Admin Application Status Update and Deletion Actions

        [HttpPost]
        public async Task<IActionResult> AdminUpdateApplicationStatus(int applicationId, string status)
        {
            var app = await _db.Applications.FindAsync(applicationId);

            if (app != null)
            {
                app.Status = status;
                _db.Applications.Update(app);
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = $"Application {status.ToLower()}.";
            return RedirectToAction("Applications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var app = await _db.Applications
                .Include(a => a.SupportingDocuments)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (app != null)
            {
                _db.Applications.Remove(app);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Application removed successfully.";
            }
            else
            {
                TempData["Success"] = "Application not found.";
            }

            return RedirectToAction("Applications");
        }






        // Admin Monitoring and Reporting Actions

        public async Task<IActionResult> MonitoringReporting()
        {
            ViewBag.TotalApplications = await _db.Applications.CountAsync();
            ViewBag.PendingApplications = await _db.Applications.CountAsync(a => a.Status == "Pending");
            ViewBag.ApprovedApplications = await _db.Applications.CountAsync(a => a.Status == "Approved");
            ViewBag.RejectedApplications = await _db.Applications.CountAsync(a => a.Status == "Rejected");
            ViewBag.ActiveInternships = await _db.Internships.CountAsync(i => i.IsActive);
            ViewBag.TotalStudents = await _db.Students.CountAsync();
            ViewBag.TotalCompanies = await _db.Companies.CountAsync();

            var recentApplications = await _db.Applications
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .OrderByDescending(a => a.AppliedAt)
                .Take(10)
                .ToListAsync();

            return View(recentApplications);
        }






        // Admin Report Generation Actions

        public async Task<IActionResult> Reports()
        {
            var vm = await BuildReportViewModel();
            return View(vm);
        }

        public async Task<IActionResult> ReportPreview()
        {
            var vm = await BuildReportViewModel();
            return View(vm);
        }

        public async Task<IActionResult> DownloadHtmlReport()
        {
            return await GenerateHtmlReportFile();
        }

        public async Task<IActionResult> DownloadReportHtml()
        {
            return await GenerateHtmlReportFile();
        }

        private async Task<IActionResult> GenerateHtmlReportFile()
        {
            var vm = await BuildReportViewModel();
            var generatedAt = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            var rows = new StringBuilder();

            foreach (var row in vm.PlacementByProgramme)
            {
                rows.Append($@"
                    <tr>
                        <td>{row.Programme}</td>
                        <td>{row.TotalStudents}</td>
                        <td>{row.Placed}</td>
                        <td>{row.Pending}</td>
                        <td>{row.Rejected}</td>
                        <td><strong>{row.Rate}</strong></td>
                    </tr>");
            }

            if (!vm.PlacementByProgramme.Any())
            {
                rows.Append("<tr><td colspan='6'>No report data available.</td></tr>");
            }

            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Admin Internship Report</title>
                <style>
                    body {{
                        margin: 0;
                        padding: 35px;
                        font-family: Arial, sans-serif;
                        background: #f7eef1;
                        color: #1d2733;
                    }}

                    .report-wrapper {{
                        background: #ffffff;
                        border-radius: 14px;
                        padding: 28px;
                        border: 1px solid #ead8de;
                    }}

                    .report-header {{
                        border-bottom: 1px solid #ead8de;
                        padding-bottom: 18px;
                        margin-bottom: 24px;
                    }}

                    .report-title {{
                        font-size: 28px;
                        font-weight: 700;
                        margin-bottom: 6px;
                    }}

                    .report-subtitle {{
                        color: #777;
                        font-size: 14px;
                    }}

                    .stats {{
                        display: grid;
                        grid-template-columns: repeat(4, 1fr);
                        gap: 16px;
                        margin-bottom: 28px;
                    }}

                    .stat-card {{
                        background: #fff;
                        border: 1px solid #ead8de;
                        border-radius: 12px;
                        padding: 18px;
                    }}

                    .stat-card span {{
                        display: block;
                        color: #777;
                        font-size: 13px;
                        margin-bottom: 10px;
                    }}

                    .stat-card strong {{
                        font-size: 26px;
                        color: #000;
                    }}

                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        background: #fff;
                        border-radius: 12px;
                        overflow: hidden;
                    }}

                    th {{
                        background: #f9f2f4;
                        color: #333;
                        text-align: left;
                        padding: 13px;
                        font-size: 13px;
                        border-bottom: 1px solid #ead8de;
                    }}

                    td {{
                        padding: 13px;
                        border-bottom: 1px solid #f1e4e8;
                        font-size: 14px;
                    }}

                    .footer {{
                        margin-top: 25px;
                        display: flex;
                        justify-content: space-between;
                        color: #666;
                        font-size: 13px;
                    }}
                </style>
            </head>
            <body>
                <div class='report-wrapper'>
                    <div class='report-header'>
                        <div class='report-title'>Internship Management Report</div>
                        <div class='report-subtitle'>Administrator summary report generated on {generatedAt}</div>
                    </div>

                    <div class='stats'>
                        <div class='stat-card'>
                            <span>Placement Rate</span>
                            <strong>{vm.PlacementRate}%</strong>
                        </div>
                        <div class='stat-card'>
                            <span>Total Students</span>
                            <strong>{vm.TotalStudents}</strong>
                        </div>
                        <div class='stat-card'>
                            <span>Active Internships</span>
                            <strong>{vm.ActiveInternships}</strong>
                        </div>
                        <div class='stat-card'>
                            <span>Approved Applications</span>
                            <strong>{vm.CompletedApplications}</strong>
                        </div>
                    </div>

                    <h2>Placement Summary by Programme</h2>

                    <table>
                        <thead>
                            <tr>
                                <th>Programme</th>
                                <th>Total Students</th>
                                <th>Placed</th>
                                <th>Pending</th>
                                <th>Rejected</th>
                                <th>Rate</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>

                    <div class='footer'>
                        <div>FYP Internship Management System</div>
                        <div>Prepared for Administrator</div>
                    </div>
                </div>
            </body>
            </html>";

            var bytes = Encoding.UTF8.GetBytes(html);
            var fileName = $"Internship_Report_{DateTime.Now:yyyyMMdd_HHmm}.html";

            return File(bytes, "text/html", fileName);
        }

        public async Task<IActionResult> DownloadCsvReport()
        {
            return await GenerateCsvReportFile();
        }

        public async Task<IActionResult> DownloadReportCsv()
        {
            return await GenerateCsvReportFile();
        }

        private async Task<IActionResult> GenerateCsvReportFile()
        {
            var vm = await BuildReportViewModel();

            var csv = new StringBuilder();
            csv.AppendLine("Programme,Total Students,Placed,Pending,Rejected,Rate");

            foreach (var row in vm.PlacementByProgramme)
            {
                csv.AppendLine($"\"{row.Programme}\",{row.TotalStudents},{row.Placed},{row.Pending},{row.Rejected},\"{row.Rate}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"Internship_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(bytes, "text/csv", fileName);
        }

        private async Task<AdminReportsViewModel> BuildReportViewModel()
        {
            var students = await _db.Students
                .Include(s => s.Applications)
                .ToListAsync();

            var allApps = await _db.Applications.ToListAsync();

            var totalStudents = students.Count;
            var placed = allApps.Count(a => a.Status == "Approved");

            var grouped = students
                .GroupBy(s => string.IsNullOrEmpty(s.Programme) ? "Unspecified" : s.Programme)
                .Select(g => new ProgrammePlacementRow
                {
                    Programme = g.Key,
                    TotalStudents = g.Count(),
                    Placed = g.Sum(s => s.Applications.Count(a => a.Status == "Approved")),
                    Pending = g.Sum(s => s.Applications.Count(a => a.Status == "Pending")),
                    Rejected = g.Sum(s => s.Applications.Count(a => a.Status == "Rejected"))
                })
                .ToList();

            return new AdminReportsViewModel
            {
                TotalStudents = totalStudents,
                ActiveInternships = await _db.Internships.CountAsync(i => i.IsActive),
                CompletedApplications = placed,
                PlacementRate = totalStudents > 0
                    ? Math.Round(placed * 100.0 / totalStudents, 1)
                    : 0,
                PlacementByProgramme = grouped
            };
        }





        // Admin Security and Authentication Actions

        public async Task<IActionResult> SecurityAuthentication()
        {
            var users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.Status == "Active");
            ViewBag.PendingUsers = users.Count(u => u.Status == "Pending");
            ViewBag.DeactivatedUsers = users.Count(u => u.Status == "Deactivated");
            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        
    }
}