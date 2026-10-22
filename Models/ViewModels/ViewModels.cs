using System.ComponentModel.DataAnnotations;
using JobPortal.Models;

namespace JobPortal.Models.ViewModels
{
    // ---- Account ViewModels ----
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        public string? Location { get; set; }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    // ---- Job ViewModels ----
    public class JobSearchViewModel
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public JobType? JobType { get; set; }
        public JobCategory? Category { get; set; }
        public string? SalaryMin { get; set; }
        public List<Job> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class JobDetailViewModel
    {
        public Job Job { get; set; } = null!;
        public bool HasApplied { get; set; }
        public bool IsOwner { get; set; }
        public int ApplicationCount { get; set; }
    }

    public class PostJobViewModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Salary Range")]
        public string? SalaryRange { get; set; }

        [Required]
        [Display(Name = "Job Type")]
        public JobType JobType { get; set; }

        [Required]
        public JobCategory Category { get; set; }

        [Display(Name = "Required Skills")]
        public string? RequiredSkills { get; set; }

        public string? Benefits { get; set; }

        [Display(Name = "Experience Required (years)")]
        [Range(0, 30)]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Application Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ApplyJobViewModel
    {
        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        [Display(Name = "Cover Letter")]
        public string? CoverLetter { get; set; }

        [Display(Name = "Resume (PDF or DOCX)")]
        public IFormFile? Resume { get; set; }

        public bool UseExistingResume { get; set; }
        public string? ExistingResumeFileName { get; set; }
    }

    // ---- Dashboard ViewModels ----
    public class SeekerDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public int TotalApplications { get; set; }
        public int PendingCount { get; set; }
        public int ShortlistedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public List<JobApplication> RecentApplications { get; set; } = new();
    }

    public class RecruiterDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();
    }

    public class ApplicantsViewModel
    {
        public Job Job { get; set; } = null!;
        public List<JobApplication> Applications { get; set; } = new();
        public string? StatusFilter { get; set; }
    }

    public class MyApplicationsViewModel
    {
        public List<JobApplication> Applications { get; set; } = new();
        public string? StatusFilter { get; set; }
    }
}
