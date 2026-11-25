using Microsoft.Data.SqlClient;
using RelAte.Api.Services;

namespace RelAte.Ingestion;

/// <summary>
/// Embedding Generation & Azure SQL Batch Upsert Worker for Pradeep (Track 2B)
/// </summary>
public class VectorEmbedder
{
    private readonly ILlmProvider _llmProvider;
    private readonly string _connectionString;

    public VectorEmbedder(ILlmProvider llmProvider, string connectionString)
    {
        _llmProvider = llmProvider;
        _connectionString = connectionString;
    }

    public static string BuildDenseText(NormalizedCandidateProfile profile)
    {
        var skillNames = profile.Skills.Select(s => s.CanonicalSkill).Where(s => !string.IsNullOrEmpty(s));
        return $"Role: {profile.CurrentRole}. Experience: {profile.TotalExperienceYears} years. Skills: {string.Join(", ", skillNames)}.";
    }

    public async Task<float[]> GenerateEmbeddingAsync(NormalizedCandidateProfile profile)
    {
        var denseText = BuildDenseText(profile);
        return await _llmProvider.GenerateEmbeddingAsync(denseText);
    }

    public async Task UpsertCandidateToAzureSqlAsync(
        ExtractedResume extracted,
        NormalizedCandidateProfile normalized,
        float[] embedding)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            Console.WriteLine($"[Dry Run] Skipping DB write: Candidate '{normalized.FullName}' with {normalized.Skills.Count} skills ready.");
            return;
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var candidateId = Guid.NewGuid();

        // 1. Insert Candidate
        const string insertCandidateSql = """
            INSERT INTO dbo.candidates (candidate_id, full_name, email, phone, linkedin_url, total_experience_years, current_or_last_role, raw_resume_text, resume_file_path)
            VALUES (@Id, @Name, @Email, @Phone, @LinkedIn, @Exp, @Role, @RawText, @Path);
            """;

        await using (var cmd = new SqlCommand(insertCandidateSql, conn))
        {
            cmd.Parameters.AddWithValue("@Id", candidateId);
            cmd.Parameters.AddWithValue("@Name", normalized.FullName);
            cmd.Parameters.AddWithValue("@Email", (object?)extracted.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)extracted.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LinkedIn", (object?)extracted.LinkedInUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Exp", normalized.TotalExperienceYears);
            cmd.Parameters.AddWithValue("@Role", (object?)normalized.CurrentRole ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RawText", extracted.RawText);
            cmd.Parameters.AddWithValue("@Path", extracted.FilePath);
            await cmd.ExecuteNonQueryAsync();
        }

        // 2. Insert Normalized Skills
        foreach (var skill in normalized.Skills)
        {
            const string insertSkillSql = """
                INSERT INTO dbo.candidate_skills (candidate_id, canonical_skill, raw_skill_mention, years_experience)
                VALUES (@CandidateId, @Canonical, @RawMention, @Years);
                """;

            await using var cmd = new SqlCommand(insertSkillSql, conn);
            cmd.Parameters.AddWithValue("@CandidateId", candidateId);
            cmd.Parameters.AddWithValue("@Canonical", skill.CanonicalSkill);
            cmd.Parameters.AddWithValue("@RawMention", (object?)skill.RawMention ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Years", (object?)skill.YearsExperience ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine($"[Success] Ingested candidate '{normalized.FullName}' (ID: {candidateId}) into Azure SQL.");
    }
}
