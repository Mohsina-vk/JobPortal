using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredJobs = await _context.Jobs
                .Include(j => j.Recruiter)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .Take(6)
                .ToListAsync();

            var totalJobs = await _context.Jobs.CountAsync(j => j.IsActive);
            var totalCompanies = await _context.Jobs
                .Where(j => j.IsActive)
                .Select(j => j.CompanyName)
                .Distinct()
                .CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();

            ViewBag.FeaturedJobs = featuredJobs;
            ViewBag.TotalJobs = totalJobs;
            ViewBag.TotalCompanies = totalCompanies;
            ViewBag.TotalApplications = totalApplications;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
