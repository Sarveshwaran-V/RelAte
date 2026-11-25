using Microsoft.AspNetCore.Mvc;
using RelAte.Api.Models;
using RelAte.Api.Services;
using System.Text.Json;

namespace RelAte.Api.Controllers;

[ApiController]
[Route("")]
public class TalentController : ControllerBase
{
    private readonly IDeterministicScoringEngine _scoringEngine;
    private readonly ILlmProvider _llmProvider;
    private readonly IWebHostEnvironment _env;

    public TalentController(
        IDeterministicScoringEngine scoringEngine,
        ILlmProvider llmProvider,
        IWebHostEnvironment env)
    {
        _scoringEngine = scoringEngine;
        _llmProvider = llmProvider;
        _env = env;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "RelAte .NET Talent Engine",
            runtime = ".NET 10 / ASP.NET Core"
        });
    }

    [HttpPost("api/parse-jd")]
    public async Task<ActionResult<ParsedJD>> ParseJobDescription([FromBody] RawJDRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.RawText))
        {
            return BadRequest(new { detail = "Job description text cannot be empty." });
        }

        var parsed = await _llmProvider.ParseJobDescriptionAsync(request.RawText);
        return Ok(parsed);
    }

    [HttpPost("api/score")]
    public ActionResult<DeterministicScoreResult> ScoreCandidate([FromBody] ScoreCandidateRequest request)
    {
        if (request == null || request.ParsedJd == null)
        {
            return BadRequest(new { detail = "Invalid request payload." });
        }

        var result = _scoringEngine.ComputeScore(
            request.CandidateSkills,
            request.CandidateExperienceYears,
            request.ParsedJd.MandatorySkills,
            request.ParsedJd.OptionalSkills,
            request.ParsedJd.MinExperienceYears
        );

        return Ok(result);
    }

    [HttpPost("api/match-candidates")]
    public ActionResult<List<CandidateMatchResponse>> MatchCandidates([FromBody] ParsedJD jd)
    {
        // Path to sample mock candidates in /contracts
        var samplePath = Path.Combine(_env.ContentRootPath, "..", "..", "contracts", "candidate_match_sample.json");
        if (System.IO.File.Exists(samplePath))
        {
            var json = System.IO.File.ReadAllText(samplePath);
            var candidates = JsonSerializer.Deserialize<List<CandidateMatchResponse>>(json);
            return Ok(candidates ?? new List<CandidateMatchResponse>());
        }

        // Fallback default sample candidates
        return Ok(new List<CandidateMatchResponse>
        {
            new()
            {
                CandidateId = "6a2f8910-b38c-42e5-b827-557766551111",
                Name = "Arun Kumar",
                LinkedinUrl = "https://www.linkedin.com/in/arunkumar-dev",
                TotalExperienceYears = 5.2,
                ConfidenceScore = 94.0,
                ScoreBreakdown = new ScoreBreakdown
                {
                    MandatoryMatched = new List<string> { "C#", ".NET Core", "SQL Server" },
                    MandatoryMissing = new List<string>(),
                    OptionalMatched = new List<string> { "Azure", "Docker" }
                },
                EvidenceSnippets = new List<string>
                {
                    "Led development of enterprise financial processing system in .NET Core 8 and C# with Microsoft SQL Server backend.",
                    "Automated CI/CD pipelines deploying Docker microservices on Azure App Services."
                },
                FitSummary = "Exceptional fit matching all mandatory (.NET Core, C#, SQL Server) and optional skills (Azure, Docker) with 5.2 years experience."
            }
        });
    }
}
