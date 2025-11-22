// Controllers/ResumeController.cs
using Microsoft.AspNetCore.Mvc;
using ResumeScreenerApp.Models;
using ResumeScreenerApp.Services;

namespace ResumeScreenerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly ResumeScoringService _scoringService;

        public ResumeController(ResumeScoringService scoringService)
        {
            _scoringService = scoringService;
        }

        [HttpPost("score")]
        public ActionResult<ScoreResult> Score([FromBody] ResumeInput input)
        {
            var result = _scoringService.Score(input);
            return Ok(result);
        }
    }
}
