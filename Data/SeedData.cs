using JobPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // Seed roles
            string[] roles = { "Recruiter", "JobSeeker" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed recruiter user
            if (await userManager.FindByEmailAsync("recruiter@techcorp.com") == null)
            {
                var recruiter = new ApplicationUser
                {
                    UserName = "recruiter@techcorp.com",
                    Email = "recruiter@techcorp.com",
                    FullName = "Alex Johnson",
                    Role = UserRole.Recruiter,
                    CompanyName = "TechCorp Solutions",
                    CompanyDescription = "Leading software development company",
                    Location = "New York, NY",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(recruiter, "Recruiter@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(recruiter, "Recruiter");

                // Seed sample jobs
                var jobs = new List<Job>
                {
                    new Job
                    {
                        Title = "Senior .NET Developer",
                        Description = "We are looking for an experienced .NET Developer to join our growing team. You will be responsible for designing, developing, and maintaining enterprise-level applications.\n\nResponsibilities:\n- Design and develop scalable .NET applications\n- Collaborate with cross-functional teams\n- Perform code reviews and mentor junior developers\n- Ensure application performance and reliability",
                        CompanyName = "TechCorp Solutions",
                        Location = "New York, NY",
                        SalaryRange = "$90,000 - $130,000",
                        JobType = JobType.FullTime,
                        Category = JobCategory.Technology,
                        RequiredSkills = ".NET, C#, ASP.NET Core, SQL Server, Entity Framework",
                        Benefits = "Health Insurance, 401K, Remote Work Options, Professional Development",
                        ExperienceYears = 5,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(30)
                    },
                    new Job
                    {
                        Title = "React Frontend Developer",
                        Description = "Join our dynamic team to build modern, responsive web applications. You'll work closely with designers and backend developers to deliver exceptional user experiences.\n\nResponsibilities:\n- Build reusable React components\n- Integrate with REST APIs\n- Optimize application performance\n- Write clean, maintainable code",
                        CompanyName = "TechCorp Solutions",
                        Location = "Remote",
                        SalaryRange = "$75,000 - $110,000",
                        JobType = JobType.Remote,
                        Category = JobCategory.Technology,
                        RequiredSkills = "React, JavaScript, TypeScript, CSS, REST APIs",
                        Benefits = "Fully Remote, Flexible Hours, Health Insurance, Stock Options",
                        ExperienceYears = 3,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(25)
                    },
                    new Job
                    {
                        Title = "UX/UI Designer",
                        Description = "We are seeking a creative UX/UI Designer to craft beautiful and intuitive user interfaces. You'll collaborate with product managers and developers to create user-centered designs.\n\nResponsibilities:\n- Create wireframes, prototypes, and high-fidelity designs\n- Conduct user research and usability testing\n- Maintain design systems\n- Present designs to stakeholders",
                        CompanyName = "TechCorp Solutions",
                        Location = "New York, NY",
                        SalaryRange = "$70,000 - $100,000",
                        JobType = JobType.FullTime,
                        Category = JobCategory.Design,
                        RequiredSkills = "Figma, Adobe XD, User Research, Prototyping, Design Systems",
                        Benefits = "Health Insurance, Creative Environment, Team Events, Gym Membership",
                        ExperienceYears = 3,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(20)
                    },
                    new Job
                    {
                        Title = "DevOps Engineer",
                        Description = "Looking for a skilled DevOps Engineer to improve our deployment pipelines and infrastructure. You will design, implement, and manage our CI/CD processes.",
                        CompanyName = "TechCorp Solutions",
                        Location = "San Francisco, CA",
                        SalaryRange = "$100,000 - $140,000",
                        JobType = JobType.FullTime,
                        Category = JobCategory.Technology,
                        RequiredSkills = "Docker, Kubernetes, Azure, CI/CD, Terraform, Linux",
                        Benefits = "Competitive Salary, Stock Options, Remote Options, Learning Budget",
                        ExperienceYears = 4,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(28)
                    },
                    new Job
                    {
                        Title = "Data Analyst Intern",
                        Description = "An exciting internship opportunity for aspiring data analysts. You will work with real datasets and gain hands-on experience in data analysis and visualization.",
                        CompanyName = "TechCorp Solutions",
                        Location = "Chicago, IL",
                        SalaryRange = "$20/hr",
                        JobType = JobType.Internship,
                        Category = JobCategory.Technology,
                        RequiredSkills = "Python, SQL, Excel, Power BI, Statistics",
                        Benefits = "Mentorship, Networking, Potential Full-Time Offer",
                        ExperienceYears = 0,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(15)
                    },
                    new Job
                    {
                        Title = "Digital Marketing Specialist",
                        Description = "We need a creative Digital Marketing Specialist to drive our online presence and campaigns. You'll manage SEO, social media, and email marketing strategies.",
                        CompanyName = "TechCorp Solutions",
                        Location = "Austin, TX",
                        SalaryRange = "$55,000 - $75,000",
                        JobType = JobType.FullTime,
                        Category = JobCategory.Marketing,
                        RequiredSkills = "SEO, Google Ads, Social Media, Email Marketing, Analytics",
                        Benefits = "Health Insurance, Flexible Hours, Performance Bonuses",
                        ExperienceYears = 2,
                        RecruiterId = recruiter.Id,
                        Deadline = DateTime.UtcNow.AddDays(22)
                    }
                };

                context.Jobs.AddRange(jobs);
                await context.SaveChangesAsync();
            }

            // Seed seeker user
            if (await userManager.FindByEmailAsync("seeker@example.com") == null)
            {
                var seeker = new ApplicationUser
                {
                    UserName = "seeker@example.com",
                    Email = "seeker@example.com",
                    FullName = "Sarah Williams",
                    Role = UserRole.JobSeeker,
                    Location = "Boston, MA",
                    ProfileSummary = "Passionate software developer with 3 years of experience in .NET and web technologies.",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(seeker, "Seeker@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(seeker, "JobSeeker");
            }
        }
    }
}
