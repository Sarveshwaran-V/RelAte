using System.Text.Json;
using System.Text.Json.Serialization;
using RelAte.Api.Services;

namespace RelAte.Ingestion;

public class CandidateSkillInfo
{
    [JsonPropertyName("canonical_skill")]
    public string CanonicalSkill { get; set; } = string.Empty;

    [JsonPropertyName("raw_mention")]
    public string RawMention { get; set; } = string.Empty;

    [JsonPropertyName("years_experience")]
    public double? YearsExperience { get; set; }
}

public class NormalizedCandidateProfile
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("total_experience_years")]
    public double TotalExperienceYears { get; set; }

    [JsonPropertyName("current_role")]
    public string CurrentRole { get; set; } = string.Empty;

    [JsonPropertyName("skills")]
    public List<CandidateSkillInfo> Skills { get; set; } = new();
}

/// <summary>
/// Skill Normalization Pipeline for Pradeep (Track 2B)
/// Standardizes variations like 'ASP.NET Core 8', 'C# developer' into canonical tags.
/// Includes exponential backoff for Gemini Flash free-tier quotas.
/// </summary>
public class SkillNormalizer
{
    private const string SystemInstruction = """
        You are a technical talent taxonomy parser.
        Given resume text, extract:
        1. Full Name (string)
        2. Total Years of Experience (number)
        3. Current or Last Job Role (string)
        4. Array of skills: each with canonical_skill (standardized name like '.NET Core', 'C#', 'SQL Server'), raw_mention (as found in resume), and years_experience (number or null).
        Return ONLY valid JSON.
        """;

    private readonly ILlmProvider _llmProvider;

    public SkillNormalizer(ILlmProvider llmProvider)
    {
        _llmProvider = llmProvider;
    }

    public async Task<NormalizedCandidateProfile> NormalizeSkillsAsync(string rawResumeText)
    {
        var truncated = rawResumeText.Length > 4000 ? rawResumeText[..4000] : rawResumeText;
        var prompt = $"Resume Content:\n\n{truncated}";

        const int maxRetries = 3;
        var delayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _llmProvider.GenerateTextAsync(prompt, SystemInstruction, temperature: 0.0);
                var clean = response.Trim();
                if (clean.StartsWith("```json")) clean = clean[7..];
                if (clean.StartsWith("```")) clean = clean[3..];
                if (clean.EndsWith("```")) clean = clean[..^3];

                var profile = JsonSerializer.Deserialize<NormalizedCandidateProfile>(clean.Trim());
                if (profile != null) return profile;
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries) throw;
                Console.WriteLine($"[Warning] Skill normalization attempt {attempt} failed: {ex.Message}. Retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }

        return new NormalizedCandidateProfile();
    }
}
