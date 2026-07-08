using FYP_InternshipManagementSystem.Data;
using FYP_InternshipManagementSystem.Models;
using FYP_InternshipManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_InternshipManagementSystem.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public StudentController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        private async Task<FYP_InternshipManagementSystem.Models.Student> GetStudentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _db.Students.Include(s => s.User)
                .FirstAsync(s => s.UserId == user!.Id);
        }

        private void SetViewBag(FYP_InternshipManagementSystem.Models.Student student)
        {
            ViewBag.StudentName = student.User.Name;
            ViewBag.StudentEmail = student.User.Email;
            ViewBag.ProfilePic = student.ProfilePic;
        }





        //  Listings 

        public async Task<IActionResult> Listings(
            string? search,
            string? location,
            string? department,
            string? allowanceRange,
            string? duration,
            string? sortBy)
        {
            var student = await GetStudentAsync();

            var query = _db.Internships
                .Include(i => i.Company).ThenInclude(c => c.User)
                .Where(i => i.IsActive);





            // Filter

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.Title.Contains(search) ||
                    i.Company.User.Name.Contains(search) ||
                    i.Company.CompanyLocation.Contains(search) ||
                    i.Department.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(i => i.Company.CompanyLocation.Contains(location));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(i => i.Department.Contains(department));
            }

            var internships = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(allowanceRange))
            {
                internships = allowanceRange switch
                {
                    "0-500" => internships.Where(i => GetFirstNumber(i.Allowance) >= 0 && GetFirstNumber(i.Allowance) <= 500).ToList(),
                    "501-800" => internships.Where(i => GetFirstNumber(i.Allowance) >= 501 && GetFirstNumber(i.Allowance) <= 800).ToList(),
                    "801-1000" => internships.Where(i => GetFirstNumber(i.Allowance) >= 801 && GetFirstNumber(i.Allowance) <= 1000).ToList(),
                    "1001-1500" => internships.Where(i => GetFirstNumber(i.Allowance) >= 1001 && GetFirstNumber(i.Allowance) <= 1500).ToList(),
                    "1501-above" => internships.Where(i => GetFirstNumber(i.Allowance) >= 1501).ToList(),
                    _ => internships
                };
            }

            if (!string.IsNullOrWhiteSpace(duration))
            {
                internships = internships
                    .Where(i => GetFirstNumber(i.Duration) == GetFirstNumber(duration))
                    .ToList();
            }

            internships = sortBy switch
            {
                "allowance-high" => internships.OrderByDescending(i => GetFirstNumber(i.Allowance)).ToList(),
                "allowance-low" => internships.OrderBy(i => GetFirstNumber(i.Allowance)).ToList(),
                "duration-short" => internships.OrderBy(i => GetFirstNumber(i.Duration)).ToList(),
                "duration-long" => internships.OrderByDescending(i => GetFirstNumber(i.Duration)).ToList(),
                _ => internships.OrderByDescending(i => i.CreatedAt).ToList()
            };

            var savedIds = await _db.SavedInternships
                .Where(si => si.StudentId == student.StudentId)
                .Select(si => si.InternshipId).ToListAsync();

            var appliedIds = await _db.Applications
                .Where(a => a.StudentId == student.StudentId)
                .Select(a => a.InternshipId).ToListAsync();

            SetViewBag(student);
            ViewBag.AllowanceRange = allowanceRange;
            ViewBag.Duration = duration;
            ViewBag.SortBy = sortBy;

            var vm = new InternshipListViewModel
            {
                Internships = internships,
                SavedInternshipIds = savedIds,
                AppliedInternshipIds = appliedIds,
                SearchQuery = search,
                Location = location,
                Department = department
            };
            return View(vm);
        }

        private static int GetFirstNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            var digits = new string(value.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var number) ? number : 0;
        }





        // Apply internship

        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var student = await GetStudentAsync();
            var internship = await _db.Internships
                .Include(i => i.Company).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(i => i.InternshipId == id);

            if (internship == null) return NotFound();

            var alreadyApplied = await _db.Applications
                .AnyAsync(a => a.StudentId == student.StudentId && a.InternshipId == id);
            if (alreadyApplied)
            {
                TempData["Error"] = "You have already applied for this internship.";
                return RedirectToAction("Listings");
            }

            SetViewBag(student);
            return View(new ApplyInternshipViewModel
            {
                InternshipId = id,
                Internship = internship,
                Student = student
            });
        }

        [HttpPost]
        public async Task<IActionResult> Apply(ApplyInternshipViewModel model)
        {
            var student = await GetStudentAsync();

            string? resumePath = student.ResumePath;
            string? resumeName = student.ResumeName;

            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "resumes");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{student.StudentId}_{Guid.NewGuid()}{Path.GetExtension(model.ResumeFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ResumeFile.CopyToAsync(stream);
                resumePath = $"/uploads/resumes/{fileName}";
                resumeName = model.ResumeFile.FileName;
                student.ResumePath = resumePath;
                student.ResumeName = resumeName;
                _db.Students.Update(student);
            }

            var application = new Application
            {
                StudentId = student.StudentId,
                InternshipId = model.InternshipId,
                CoverLetter = model.CoverLetter,
                AppliedAt = DateTime.Now,
                Status = "Pending",
                ResumePathSnapshot = resumePath,
                ResumeNameSnapshot = resumeName
            };
            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            if (model.SupportingDocuments != null)
            {
                var docsDir = Path.Combine(_env.WebRootPath, "uploads", "documents");
                Directory.CreateDirectory(docsDir);

                foreach (var file in model.SupportingDocuments.Where(f => f.Length > 0))
                {
                    var fileName = $"{application.ApplicationId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(docsDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _db.SupportingDocuments.Add(new SupportingDocument
                    {
                        ApplicationId = application.ApplicationId,
                        SupportingDocName = file.FileName,
                        SupportingDocPath = $"/uploads/documents/{fileName}"
                    });
                }
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Application submitted successfully!";
            return RedirectToAction("MyApplication");
        }





        // Student Application 

        public async Task<IActionResult> MyApplication()
        {
            var student = await GetStudentAsync();
            var applications = await _db.Applications
                .Include(a => a.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .Include(a => a.SupportingDocuments)
                .Where(a => a.StudentId == student.StudentId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            SetViewBag(student);
            return View(applications);
        }

        public async Task<IActionResult> ApplicationDetail(int id)
        {
            var student = await GetStudentAsync();
            var application = await _db.Applications
                .Include(a => a.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .Include(a => a.SupportingDocuments)
                .FirstOrDefaultAsync(a => a.ApplicationId == id && a.StudentId == student.StudentId);

            if (application == null) return NotFound();

            SetViewBag(student);
            return View(application);
        }





        //  Saved Listings Management 

        public async Task<IActionResult> SavedListing()
        {
            var student = await GetStudentAsync();
            var saved = await _db.SavedInternships
                .Include(si => si.Internship).ThenInclude(i => i.Company).ThenInclude(c => c.User)
                .Where(si => si.StudentId == student.StudentId)
                .OrderByDescending(si => si.SavedAt)
                .ToListAsync();

            SetViewBag(student);
            return View(saved);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSave(int internshipId)
        {
            var student = await GetStudentAsync();
            var existing = await _db.SavedInternships
                .FirstOrDefaultAsync(si => si.StudentId == student.StudentId && si.InternshipId == internshipId);

            if (existing != null)
                _db.SavedInternships.Remove(existing);
            else
                _db.SavedInternships.Add(new SavedInternship { StudentId = student.StudentId, InternshipId = internshipId });

            await _db.SaveChangesAsync();
            return RedirectToAction("Listings");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSave(int internshipId)
        {
            var student = await GetStudentAsync();
            var existing = await _db.SavedInternships
                .FirstOrDefaultAsync(si => si.StudentId == student.StudentId && si.InternshipId == internshipId);

            if (existing != null)
            {
                _db.SavedInternships.Remove(existing);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("SavedListing");
        }





        //  Profile Management 

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var student = await GetStudentAsync();
            var vm = new StudentProfileViewModel
            {
                Name = student.User.Name,
                Email = student.User.Email!,
                ContactNumber = student.ContactNumber,
                EducationalInstitution = student.EducationalInstitution,
                Programme = student.Programme,
                CGPA = student.CGPA,
                ProfilePicPath = student.ProfilePic,
                ResumeName = student.ResumeName,
                ResumePath = student.ResumePath
            };
            SetViewBag(student);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(StudentProfileViewModel model)
        {
            ModelState.Remove("ProfilePicFile");
            ModelState.Remove("ResumeFile");
            ModelState.Remove("ProfilePicPath");
            ModelState.Remove("ResumePath");
            ModelState.Remove("ResumeName");
            ModelState.Remove("Email");

            var student = await GetStudentAsync();

            if (!ModelState.IsValid)
            {
                model.ProfilePicPath = student.ProfilePic;
                model.ResumeName = student.ResumeName;
                model.ResumePath = student.ResumePath;
                model.Email = student.User.Email!;
                SetViewBag(student);
                return View(model);
            }

            var user = student.User;
            user.Name = model.Name;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.ContactNumber))
                student.ContactNumber = model.ContactNumber;

            if (!string.IsNullOrWhiteSpace(model.Programme))
                student.Programme = model.Programme;

            if (model.CGPA != null)
                student.CGPA = model.CGPA;

            if (!string.IsNullOrWhiteSpace(model.EducationalInstitution))
                student.EducationalInstitution = model.EducationalInstitution;




            // profile pic upload 

            if (model.ProfilePicFile != null && model.ProfilePicFile.Length > 0)
            {
                try
                {
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(dir);
                    var fileName = $"student_{student.StudentId}_{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicFile.FileName)}";
                    var fullPath = Path.Combine(dir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ProfilePicFile.CopyToAsync(stream);
                    }
                    student.ProfilePic = $"/uploads/profiles/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Profile photo upload failed: {ex.Message}";
                }
            }




            // resume upload

            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                try
                {
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "resumes");
                    Directory.CreateDirectory(dir);
                    var fileName = $"{student.StudentId}_{Guid.NewGuid()}{Path.GetExtension(model.ResumeFile.FileName)}";
                    var fullPath = Path.Combine(dir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ResumeFile.CopyToAsync(stream);
                    }
                    student.ResumePath = $"/uploads/resumes/{fileName}";
                    student.ResumeName = model.ResumeFile.FileName;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Resume upload failed: {ex.Message}";
                }
            }

            _db.Students.Update(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all fields correctly.";
                return RedirectToAction("Profile");
            }
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user!, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            else
                TempData["Success"] = "Password changed successfully!";

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
            var signInManager = HttpContext.RequestServices
                .GetRequiredService<SignInManager<ApplicationUser>>();
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
