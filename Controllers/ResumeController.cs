// Controllers/ResumeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeScreenerApp.Models;
using ResumeScreenerApp.Services;
using ResumeScreenerApp.Data;

namespace ResumeScreenerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly ResumeScoringService _scoringService;
        private readonly ApplicationDbContext _context;
        private readonly PdfProcessingService _pdfService;

        public ResumeController(ResumeScoringService scoringService, ApplicationDbContext context, PdfProcessingService pdfService)
        {
            _scoringService = scoringService;
            _context = context;
            _pdfService = pdfService;
        }

        [HttpPost("score")]
        public async Task<ActionResult<ScoreResult>> Score([FromBody] ResumeInput input)
        {
            var result = await _scoringService.ScoreAsync(input);
            return Ok(result);
        }

        [HttpPost("score-pdf")]
        public async Task<ActionResult<ScoreResult>> ScorePdf([FromForm] ResumeInput input)
        {
            try
            {
                if (input.ResumeFile != null)
                {
                    input.ResumeText = await _pdfService.ExtractTextFromPdfAsync(input.ResumeFile);
                }

                if (string.IsNullOrWhiteSpace(input.ResumeText))
                {
                    return BadRequest("Resume text is required (either as text or PDF file)");
                }

                var result = await _scoringService.ScoreAsync(input);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error processing PDF file");
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<Resume>>> GetHistory()
        {
            var resumes = await _context.Resumes
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync();
            return Ok(resumes);
        }
    }
}
