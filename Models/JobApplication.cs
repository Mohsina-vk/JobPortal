using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public enum ApplicationStatus { Pending, UnderReview, Shortlisted, Accepted, Rejected }

    public class JobApplication
    {
        public int Id { get; set; }

        // FK to Job
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public Job? Job { get; set; }

        // FK to Applicant
        public string ApplicantId { get; set; } = string.Empty;
        [ForeignKey("ApplicantId")]
        public ApplicationUser? Applicant { get; set; }

        public string? CoverLetter { get; set; }
        public string? ResumeFileName { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string? RecruiterNotes { get; set; }
    }
}
