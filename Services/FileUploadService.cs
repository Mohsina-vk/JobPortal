namespace JobPortal.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadResumeAsync(IFormFile file);
        bool DeleteResume(string fileName);
        string GetResumeUrl(string fileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileUploadService> _logger;
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".doc" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> UploadResumeAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("File size exceeds the 5 MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("Only PDF, DOC, and DOCX files are allowed.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Resume uploaded: {File}", uniqueName);
            return uniqueName;
        }

        public bool DeleteResume(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "uploads", "resumes", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public string GetResumeUrl(string fileName)
            => $"/uploads/resumes/{fileName}";
    }
}
