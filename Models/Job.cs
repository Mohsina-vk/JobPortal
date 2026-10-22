using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public enum JobType { FullTime, PartTime, Remote, Contract, Internship }
    public enum JobCategory
    {
        Technology, Healthcare, Finance, Education, Marketing,
        Engineering, Design, Sales, HumanResources, Operations, Other
    }

    public class Job
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public string? SalaryRange { get; set; }

        public JobType JobType { get; set; }
        public JobCategory Category { get; set; }

        public string? RequiredSkills { get; set; }
        public string? Benefits { get; set; }
        public int? ExperienceYears { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; } = true;

        // Recruiter FK
        public string RecruiterId { get; set; } = string.Empty;
        [ForeignKey("RecruiterId")]
        public ApplicationUser? Recruiter { get; set; }

        // Navigation
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        public string JobTypeDisplay => JobType switch
        {
            JobType.FullTime => "Full-Time",
            JobType.PartTime => "Part-Time",
            JobType.Remote => "Remote",
            JobType.Contract => "Contract",
            JobType.Internship => "Internship",
            _ => "Unknown"
        };
    }
}
