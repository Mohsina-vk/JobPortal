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
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IFileUploadService _fileUpload;

        public JobsController(
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

        // GET /jobs
        public async Task<IActionResult> Index(JobSearchViewModel search)
        {
            var query = _context.Jobs
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Keyword))
                query = query.Where(j =>
                    j.Title.Contains(search.Keyword) ||
                    j.Description.Contains(search.Keyword) ||
                    j.CompanyName.Contains(search.Keyword) ||
                    (j.RequiredSkills != null && j.RequiredSkills.Contains(search.Keyword)));

            if (!string.IsNullOrWhiteSpace(search.Location))
                query = query.Where(j => j.Location.Contains(search.Location));

            if (search.JobType.HasValue)
                query = query.Where(j => j.JobType == search.JobType);

            if (search.Category.HasValue)
                query = query.Where(j => j.Category == search.Category);

            search.TotalCount = await query.CountAsync();
            search.Jobs = await query
                .OrderByDescending(j => j.PostedAt)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            return View(search);
        }

        // GET /jobs/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var hasApplied = userId != null &&
                await _context.JobApplications.AnyAsync(a => a.JobId == id && a.ApplicantId == userId);

            var vm = new JobDetailViewModel
            {
                Job = job,
                HasApplied = hasApplied,
                IsOwner = job.RecruiterId == userId,
                ApplicationCount = job.Applications.Count
            };
            return View(vm);
        }

        // GET /jobs/apply/{id}
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null || !job.IsActive) return NotFound();

            var userId = _userManager.GetUserId(User);
            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == id && a.ApplicantId == userId);
            if (alreadyApplied)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            return View(new ApplyJobViewModel
            {
                JobId = id,
                Job = job,
                ExistingResumeFileName = user?.ResumeFileName
            });
        }

        // POST /jobs/apply/{id}
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(ApplyJobViewModel model)
        {
            var job = await _context.Jobs.FindAsync(model.JobId);
            if (job == null) return NotFound();
            model.Job = job;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            string? resumeFileName = null;

            if (model.Resume != null && model.Resume.Length > 0)
            {
                try
                {
                    resumeFileName = await _fileUpload.UploadResumeAsync(model.Resume);
                    // Save as user's default resume
                    user.ResumeFileName = resumeFileName;
                    await _userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Resume", ex.Message);
                    return View(model);
                }
            }
            else if (model.UseExistingResume && !string.IsNullOrEmpty(user.ResumeFileName))
            {
                resumeFileName = user.ResumeFileName;
            }

            var application = new JobApplication
            {
                JobId = model.JobId,
                ApplicantId = user.Id,
                CoverLetter = model.CoverLetter,
                ResumeFileName = resumeFileName,
                AppliedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            // Send confirmation email
            await _emailService.SendApplicationConfirmationAsync(user.Email!, user.FullName, job.Title, job.CompanyName);

            TempData["Success"] = "Application submitted successfully! Good luck! 🍀";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
