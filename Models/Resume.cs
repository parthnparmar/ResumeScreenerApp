// Models/Resume.cs
using System.ComponentModel.DataAnnotations;

namespace ResumeScreenerApp.Models
{
    public class Resume
    {
        public int Id { get; set; }
        
        [Required]
        public string ResumeText { get; set; } = string.Empty;
        
        [Required]
        public string JobDescription { get; set; } = string.Empty;
        
        public float Score { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}