using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public enum UserRole { JobSeeker, Recruiter }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? ResumeFileName { get; set; }
        public string? ProfileSummary { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool NotifyNewJobs { get; set; } = true;

        // Navigation
        public ICollection<Job> PostedJobs { get; set; } = new List<Job>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
