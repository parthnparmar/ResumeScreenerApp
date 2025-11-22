// Models/ResumeInput.cs
namespace ResumeScreenerApp.Models
{
    public class ResumeInput
    {
        public string ResumeText { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public IFormFile? ResumeFile { get; set; }
    }
}