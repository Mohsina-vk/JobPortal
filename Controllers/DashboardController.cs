using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.Role == UserRole.Recruiter)
            {
                var jobs = await _context.Jobs
                    .Include(j => j.Applications)
                    .Where(j => j.RecruiterId == user.Id)
                    .OrderByDescending(j => j.PostedAt)
                    .ToListAsync();

                var recentApps = await _context.JobApplications
                    .Include(a => a.Job)
                    .Include(a => a.Applicant)
                    .Where(a => a.Job!.RecruiterId == user.Id)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .ToListAsync();

                var vm = new RecruiterDashboardViewModel
                {
                    User = user,
                    TotalJobs = jobs.Count,
                    ActiveJobs = jobs.Count(j => j.IsActive),
                    TotalApplications = jobs.Sum(j => j.Applications.Count),
                    PendingApplications = jobs.Sum(j => j.Applications.Count(a => a.Status == ApplicationStatus.Pending)),
                    RecentJobs = jobs.Take(5).ToList(),
                    RecentApplications = recentApps
                };
                return View("RecruiterDashboard", vm);
            }
            else
            {
                var applications = await _context.JobApplications
                    .Include(a => a.Job)
                        .ThenInclude(j => j!.Recruiter)
                    .Where(a => a.ApplicantId == user.Id)
                    .OrderByDescending(a => a.AppliedAt)
                    .ToListAsync();

                var vm = new SeekerDashboardViewModel
                {
                    User = user,
                    TotalApplications = applications.Count,
                    PendingCount = applications.Count(a => a.Status == ApplicationStatus.Pending),
                    ShortlistedCount = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
                    AcceptedCount = applications.Count(a => a.Status == ApplicationStatus.Accepted),
                    RejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                    RecentApplications = applications.Take(5).ToList()
                };
                return View("SeekerDashboard", vm);
            }
        }

        public async Task<IActionResult> MyApplications(string? statusFilter = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != UserRole.JobSeeker) return Forbid();

            var query = _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Recruiter)
                .Where(a => a.ApplicantId == user.Id);

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ApplicationStatus>(statusFilter, out var status))
                query = query.Where(a => a.Status == status);

            var applications = await query.OrderByDescending(a => a.AppliedAt).ToListAsync();

            return View(new MyApplicationsViewModel
            {
                Applications = applications,
                StatusFilter = statusFilter
            });
        }
    }
}
