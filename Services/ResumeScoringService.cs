// Services/ResumeScoringService.cs
using Microsoft.ML;
using ResumeScreenerApp.Models;
using ResumeScreenerApp.MLModel;
using ResumeScreenerApp.Data;

namespace ResumeScreenerApp.Services
{
    public class ResumeScoringService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
        private readonly ApplicationDbContext _context;

        public ResumeScoringService(ApplicationDbContext context)
        {
            _context = context;
            _mlContext = new MLContext();
            
            // Check if model file exists, if not create a simple scoring logic
            if (File.Exists("MLModel/ResumeScorer.mlmodel"))
            {
                var model = _mlContext.Model.Load("MLModel/ResumeScorer.mlmodel", out _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            }
            else
            {
                _predictionEngine = null!;
            }
        }

        public async Task<ScoreResult> ScoreAsync(ResumeInput input)
        {
            float score;
            
            if (_predictionEngine != null)
            {
                var combined = input.ResumeText + " " + input.JobDescription;
                var modelInput = new ModelInput { CombinedText = combined };
                var result = _predictionEngine.Predict(modelInput);
                score = result.Score;
            }
            else
            {
                // Simple scoring logic when ML model is not available
                score = CalculateSimpleScore(input.ResumeText, input.JobDescription);
            }

            // Save to database
            var resume = new Resume
            {
                ResumeText = input.ResumeText,
                JobDescription = input.JobDescription,
                Score = score
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            return new ScoreResult { Score = (float)Math.Round(score, 1) };
        }

        private static float CalculateSimpleScore(string resumeText, string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(resumeText) || string.IsNullOrWhiteSpace(jobDescription))
                return 0f;

            var resumeText_lower = resumeText.ToLower();
            var jobDescription_lower = jobDescription.ToLower();
            
            // Extract key terms and skills
            var skillKeywords = ExtractSkills(jobDescription_lower);
            var experienceKeywords = ExtractExperience(jobDescription_lower);
            var educationKeywords = ExtractEducation(jobDescription_lower);
            
            float skillScore = CalculateKeywordMatch(resumeText_lower, skillKeywords) * 0.5f;
            float experienceScore = CalculateKeywordMatch(resumeText_lower, experienceKeywords) * 0.3f;
            float educationScore = CalculateKeywordMatch(resumeText_lower, educationKeywords) * 0.2f;
            
            // Add bonus for common professional terms
            float bonusScore = CalculateBonusScore(resumeText_lower, jobDescription_lower);
            
            float totalScore = (skillScore + experienceScore + educationScore + bonusScore) * 100f;
            
            // Ensure score is between 0 and 100
            return Math.Min(100f, Math.Max(0f, totalScore));
        }
        
        private static List<string> ExtractSkills(string text)
        {
            var commonSkills = new List<string>
            {
                "python", "java", "javascript", "c#", "sql", "html", "css", "react", "angular", "vue",
                "node.js", "express", "mongodb", "mysql", "postgresql", "git", "docker", "kubernetes",
                "aws", "azure", "gcp", "machine learning", "ai", "data analysis", "excel", "powerbi",
                "tableau", "agile", "scrum", "project management", "leadership", "communication",
                "problem solving", "teamwork", "analytical", "creative", "detail-oriented"
            };
            
            return commonSkills.Where(skill => text.Contains(skill)).ToList();
        }
        
        private static List<string> ExtractExperience(string text)
        {
            var experienceTerms = new List<string>
            {
                "years", "experience", "worked", "developed", "managed", "led", "created", "implemented",
                "designed", "built", "maintained", "optimized", "improved", "collaborated", "coordinated",
                "supervised", "trained", "mentored", "achieved", "delivered", "successful", "project"
            };
            
            return experienceTerms.Where(term => text.Contains(term)).ToList();
        }
        
        private static List<string> ExtractEducation(string text)
        {
            var educationTerms = new List<string>
            {
                "degree", "bachelor", "master", "phd", "diploma", "certificate", "university", "college",
                "education", "graduated", "gpa", "honors", "cum laude", "magna cum laude", "summa cum laude",
                "computer science", "engineering", "business", "marketing", "finance", "accounting"
            };
            
            return educationTerms.Where(term => text.Contains(term)).ToList();
        }
        
        private static float CalculateKeywordMatch(string resumeText, List<string> keywords)
        {
            if (keywords.Count == 0) return 0f;
            
            int matchCount = keywords.Count(keyword => resumeText.Contains(keyword));
            return (float)matchCount / keywords.Count;
        }
        
        private static float CalculateBonusScore(string resumeText, string jobDescription)
        {
            float bonus = 0f;
            
            // Bonus for resume length (indicates detailed experience)
            if (resumeText.Length > 500) bonus += 0.1f;
            if (resumeText.Length > 1000) bonus += 0.1f;
            
            // Bonus for specific achievements
            var achievementWords = new[] { "achieved", "increased", "improved", "reduced", "saved", "generated", "award", "recognition" };
            bonus += achievementWords.Count(word => resumeText.Contains(word)) * 0.05f;
            
            // Bonus for quantifiable results
            if (System.Text.RegularExpressions.Regex.IsMatch(resumeText, @"\d+%")) bonus += 0.1f;
            if (System.Text.RegularExpressions.Regex.IsMatch(resumeText, @"\$\d+")) bonus += 0.1f;
            
            return Math.Min(0.3f, bonus); // Cap bonus at 30%
        }
    }
}
