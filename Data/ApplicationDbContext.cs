// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using ResumeScreenerApp.Models;

namespace ResumeScreenerApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Resume> Resumes { get; set; }
    }
}