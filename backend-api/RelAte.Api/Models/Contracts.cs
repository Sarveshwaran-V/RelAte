using System.Text.Json.Serialization;

namespace RelAte.Api.Models;

/// <summary>
/// Contract A: Output from Priya's JD Parser consumed by Hybrid Retrieval & Scoring
/// </summary>
public class ParsedJD
{
    [JsonPropertyName("role_title")]
    public string RoleTitle { get; set; } = string.Empty;

    [JsonPropertyName("mandatory_skills")]
    public List<string> MandatorySkills { get; set; } = new();

    [JsonPropertyName("optional_skills")]
    public List<string> OptionalSkills { get; set; } = new();

    [JsonPropertyName("min_experience_years")]
    public double MinExperienceYears { get; set; } = 0.0;
}

public class ScoreBreakdown
{
    [JsonPropertyName("mandatory_matched")]
    public List<string> MandatoryMatched { get; set; } = new();

    [JsonPropertyName("mandatory_missing")]
    public List<string> MandatoryMissing { get; set; } = new();

    [JsonPropertyName("optional_matched")]
    public List<string> OptionalMatched { get; set; } = new();
}

public class PointsDetail
{
    [JsonPropertyName("mandatory_points")]
    public double MandatoryPoints { get; set; }

    [JsonPropertyName("optional_points")]
    public double OptionalPoints { get; set; }

    [JsonPropertyName("experience_points")]
    public double ExperiencePoints { get; set; }

    [JsonPropertyName("penalty_applied")]
    public bool PenaltyApplied { get; set; }
}

/// <summary>
/// Contract B: Ranked candidate match output from Backend/Scorer consumed by Pavanesh's UI
/// </summary>
public class CandidateMatchResponse
{
    [JsonPropertyName("candidate_id")]
    public string CandidateId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("linkedin_url")]
    public string LinkedinUrl { get; set; } = string.Empty;

    [JsonPropertyName("total_experience_years")]
    public double TotalExperienceYears { get; set; }

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("score_breakdown")]
    public ScoreBreakdown ScoreBreakdown { get; set; } = new();

    [JsonPropertyName("evidence_snippets")]
    public List<string> EvidenceSnippets { get; set; } = new();

    [JsonPropertyName("fit_summary")]
    public string FitSummary { get; set; } = string.Empty;
}

public class DeterministicScoreResult
{
    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("score_breakdown")]
    public ScoreBreakdown ScoreBreakdown { get; set; } = new();

    [JsonPropertyName("points_detail")]
    public PointsDetail PointsDetail { get; set; } = new();
}

public class RawJDRequest
{
    [JsonPropertyName("raw_text")]
    public string RawText { get; set; } = string.Empty;
}

public class ScoreCandidateRequest
{
    [JsonPropertyName("candidate_skills")]
    public List<string> CandidateSkills { get; set; } = new();

    [JsonPropertyName("candidate_experience_years")]
    public double CandidateExperienceYears { get; set; }

    [JsonPropertyName("parsed_jd")]
    public ParsedJD ParsedJd { get; set; } = new();
}
