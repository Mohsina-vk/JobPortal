using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Models.ViewModels;
using JobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class RecruiterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IFileUploadService _fileUpload;

        public RecruiterController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IFileUploadService fileUpload)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _fileUpload = fileUpload;
        }

        // GET /recruiter/jobs
        public async Task<IActionResult> MyJobs()
        {
            var userId = _userManager.GetUserId(User);
            var jobs = await _context.Jobs
                .Include(j => j.Applications)
                .Where(j => j.RecruiterId == userId)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
            return View(jobs);
        }

        // GET /recruiter/post
        public IActionResult PostJob()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return View(new PostJobViewModel { CompanyName = user?.CompanyName ?? "" });
        }

        // POST /recruiter/post
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                CompanyName = model.CompanyName,
                Location = model.Location,
                SalaryRange = model.SalaryRange,
                JobType = model.JobType,
                Category = model.Category,
                RequiredSkills = model.RequiredSkills,
                Benefits = model.Benefits,
                ExperienceYears = model.ExperienceYears,
                Deadline = model.Deadline,
                IsActive = model.IsActive,
                RecruiterId = user.Id,
                PostedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Notify all job seekers who opted in
            var seekers = await _userManager.GetUsersInRoleAsync("JobSeeker");
            var notifyList = seekers.Where(s => s.NotifyNewJobs && !string.IsNullOrEmpty(s.Email)).ToList();
            var jobUrl = Url.Action("Detail", "Jobs", new { id = job.Id }, Request.Scheme)!;

            foreach (var seeker in notifyList)
            {
                _ = _emailService.SendJobNotificationAsync(seeker.Email!, seeker.FullName, job.Title, job.CompanyName, jobUrl);
            }

            TempData["Success"] = $"Job '{job.Title}' posted successfully! {notifyList.Count} seekers notified.";
            return RedirectToAction(nameof(MyJobs));
        }

        // GET /recruiter/edit/{id}
        public async Task<IActionResult> EditJob(int id)
        {
            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == userId);
            if (job == null) return NotFound();

            return View(new PostJobViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                CompanyName = job.CompanyName,
                Location = job.Location,
                SalaryRange = job.SalaryRange,
                JobType = job.JobType,
                Category = job.Category,
                RequiredSkills = job.RequiredSkills,
                Benefits = job.Benefits,
                ExperienceYears = job.ExperienceYears,
                Deadline = job.Deadline,
                IsActive = job.IsActive
            });
        }

        // POST /recruiter/edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(PostJobViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.Id && j.RecruiterId == userId);
            if (job == null) return NotFound();

            job.Title = model.Title;
            job.Description = model.Description;
            job.CompanyName = model.CompanyName;
            job.Location = model.Location;
            job.SalaryRange = model.SalaryRange;
            job.JobType = model.JobType;
            job.Category = model.Category;
            job.RequiredSkills = model.RequiredSkills;
            job.Benefits = model.Benefits;
            job.ExperienceYears = model.ExperienceYears;
            job.Deadline = model.Deadline;
            job.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Job updated successfully.";
            return RedirectToAction(nameof(MyJobs));
        }

        // POST /recruiter/delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == userId);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job deleted.";
            return RedirectToAction(nameof(MyJobs));
        }

        // POST /recruiter/toggle/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == userId);
            if (job == null) return NotFound();

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = job.IsActive ? "Job is now active." : "Job is now inactive.";
            return RedirectToAction(nameof(MyJobs));
        }

        // GET /recruiter/applicants/{jobId}
        public async Task<IActionResult> Applicants(int jobId, string? statusFilter = null)
        {
            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs
                .Include(j => j.Applications)
                    .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == userId);

            if (job == null) return NotFound();

            var applications = job.Applications.AsEnumerable();
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ApplicationStatus>(statusFilter, out var status))
                applications = applications.Where(a => a.Status == status);

            return View(new ApplicantsViewModel
            {
                Job = job,
                Applications = applications.OrderByDescending(a => a.AppliedAt).ToList(),
                StatusFilter = statusFilter
            });
        }

        // POST /recruiter/updatestatus
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, ApplicationStatus status, string? notes = null)
        {
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var job = await _context.Jobs.FindAsync(application.JobId);
            if (job?.RecruiterId != userId) return Forbid();

            application.Status = status;
            application.RecruiterNotes = notes;
            await _context.SaveChangesAsync();

            // Notify applicant
            if (application.Applicant?.Email != null)
            {
                _ = _emailService.SendApplicationStatusUpdateAsync(
                    application.Applicant.Email,
                    application.Applicant.FullName,
                    application.Job!.Title,
                    status.ToString());
            }

            TempData["Success"] = $"Status updated to {status}.";
            return RedirectToAction(nameof(Applicants), new { jobId = application.JobId });
        }
    }
}
