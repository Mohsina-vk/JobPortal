using MailKit.Net.Smtp;
using MimeKit;

namespace JobPortal.Services
{
    public class EmailSettings
    {
        public string From { get; set; } = "noreply@jobportal.com";
        public string FromName { get; set; } = "JobPortal";
        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 25;
        public bool UseSSL { get; set; } = false;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool WriteToFolder { get; set; } = true;
        public string FolderPath { get; set; } = "EmailPickup";
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string toName, string subject, string htmlBody);
        Task SendJobNotificationAsync(string to, string toName, string jobTitle, string companyName, string jobUrl);
        Task SendApplicationConfirmationAsync(string to, string toName, string jobTitle, string companyName);
        Task SendApplicationStatusUpdateAsync(string to, string toName, string jobTitle, string status);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailService(EmailSettings settings, ILogger<EmailService> logger, IWebHostEnvironment env)
        {
            _settings = settings;
            _logger = logger;
            _env = env;
        }

        public async Task SendEmailAsync(string to, string toName, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
            message.To.Add(new MailboxAddress(toName, to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            if (_settings.WriteToFolder)
            {
                // Write to pickup folder instead of actual SMTP
                var folderPath = Path.Combine(_env.ContentRootPath, _settings.FolderPath);
                Directory.CreateDirectory(folderPath);
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.eml";
                await File.WriteAllTextAsync(
                    Path.Combine(folderPath, fileName),
                    $"To: {to}\r\nSubject: {subject}\r\n\r\n{htmlBody}"
                );
                _logger.LogInformation("Email written to pickup folder: {File}", fileName);
                return;
            }

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, _settings.UseSSL);
                if (!string.IsNullOrEmpty(_settings.Username))
                    await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
            }
        }

        public async Task SendJobNotificationAsync(string to, string toName, string jobTitle, string companyName, string jobUrl)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: Arial, sans-serif; background: #0f0f1a; color: #fff; margin: 0; padding: 0; }}
  .container {{ max-width: 600px; margin: 40px auto; background: #1a1a2e; border-radius: 16px; overflow: hidden; }}
  .header {{ background: linear-gradient(135deg, #6c63ff, #3b82f6); padding: 40px; text-align: center; }}
  .header h1 {{ margin: 0; font-size: 24px; }}
  .body {{ padding: 40px; }}
  .job-card {{ background: #16213e; border-radius: 12px; padding: 24px; margin: 20px 0; border-left: 4px solid #6c63ff; }}
  .btn {{ display: inline-block; background: linear-gradient(135deg, #6c63ff, #3b82f6); color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; margin-top: 20px; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>🚀 New Job Opportunity!</h1>
      <p style='margin:8px 0 0;opacity:0.8'>JobPortal — Your Career Partner</p>
    </div>
    <div class='body'>
      <p>Hi {toName},</p>
      <p>A new job matching your interests has been posted:</p>
      <div class='job-card'>
        <h2 style='margin:0 0 8px;color:#6c63ff'>{jobTitle}</h2>
        <p style='margin:0;color:#aaa'>🏢 {companyName}</p>
      </div>
      <a href='{jobUrl}' class='btn'>View Job & Apply</a>
      <p style='margin-top:24px;color:#aaa;font-size:14px'>To unsubscribe from notifications, update your profile settings.</p>
    </div>
    <div class='footer'>© 2024 JobPortal. All rights reserved.</div>
  </div>
</body>
</html>";
            await SendEmailAsync(to, toName, $"New Job: {jobTitle} at {companyName}", html);
        }

        public async Task SendApplicationConfirmationAsync(string to, string toName, string jobTitle, string companyName)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: Arial, sans-serif; background: #0f0f1a; color: #fff; }}
  .container {{ max-width: 600px; margin: 40px auto; background: #1a1a2e; border-radius: 16px; overflow: hidden; }}
  .header {{ background: linear-gradient(135deg, #10b981, #059669); padding: 40px; text-align: center; }}
  .header h1 {{ margin: 0; font-size: 24px; }}
  .body {{ padding: 40px; }}
  .check {{ font-size: 48px; text-align: center; margin: 20px 0; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>✅ Application Submitted!</h1>
    </div>
    <div class='body'>
      <p>Hi {toName},</p>
      <p>Your application has been successfully submitted for:</p>
      <div style='background:#16213e;border-radius:12px;padding:24px;margin:20px 0;border-left:4px solid #10b981'>
        <h2 style='margin:0 0 8px;color:#10b981'>{jobTitle}</h2>
        <p style='margin:0;color:#aaa'>🏢 {companyName}</p>
      </div>
      <p>The recruiter will review your application and update the status. You'll receive another email when there's an update.</p>
      <p style='color:#aaa'>Good luck! 🍀</p>
    </div>
    <div class='footer'>© 2024 JobPortal. All rights reserved.</div>
  </div>
</body>
</html>";
            await SendEmailAsync(to, toName, $"Application Confirmed: {jobTitle}", html);
        }

        public async Task SendApplicationStatusUpdateAsync(string to, string toName, string jobTitle, string status)
        {
            var statusColor = status switch
            {
                "Accepted" => "#10b981",
                "Rejected" => "#ef4444",
                "Shortlisted" => "#f59e0b",
                _ => "#6c63ff"
            };
            var statusEmoji = status switch
            {
                "Accepted" => "🎉",
                "Rejected" => "😔",
                "Shortlisted" => "⭐",
                _ => "📋"
            };
            var html = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: Arial, sans-serif; background: #0f0f1a; color: #fff; }}
  .container {{ max-width: 600px; margin: 40px auto; background: #1a1a2e; border-radius: 16px; overflow: hidden; }}
  .header {{ background: linear-gradient(135deg, {statusColor}, {statusColor}aa); padding: 40px; text-align: center; }}
  .body {{ padding: 40px; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>{statusEmoji} Application Update</h1>
    </div>
    <div class='body'>
      <p>Hi {toName},</p>
      <p>Your application for <strong>{jobTitle}</strong> has been updated:</p>
      <div style='background:#16213e;border-radius:12px;padding:24px;margin:20px 0;text-align:center'>
        <span style='font-size:32px;font-weight:bold;color:{statusColor}'>{status}</span>
      </div>
      <p>Log in to JobPortal to view more details and continue your job search.</p>
    </div>
    <div class='footer'>© 2024 JobPortal. All rights reserved.</div>
  </div>
</body>
</html>";
            await SendEmailAsync(to, toName, $"Application Update: {jobTitle} — {status}", html);
        }
    }
}
