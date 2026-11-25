using System.Text.RegularExpressions;
using RelAte.Api.Models;

namespace RelAte.Api.Services;

/// <summary>
/// Deterministic Confidence Scoring Engine
/// Strictly JD-Scoped, Mathematically Explainable & Reproducible
/// Owner: Sarvesh (Track 1)
/// </summary>
public class DeterministicScoringEngine : IDeterministicScoringEngine
{
    private static readonly Regex WhitespaceRegex = new(@"[\s\-_]+", RegexOptions.Compiled);

    public string NormalizeSkill(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
            return string.Empty;

        var cleaned = skill.Trim().ToLowerInvariant();
        return WhitespaceRegex.Replace(cleaned, " ");
    }

    public (List<string> Matched, List<string> Missing) CalculateSkillOverlap(
        List<string> candidateSkills,
        List<string> targetSkills)
    {
        var matched = new List<string>();
        var missing = new List<string>();

        if (targetSkills == null || targetSkills.Count == 0)
            return (matched, missing);

        var candidateNormSet = (candidateSkills ?? new List<string>())
            .Select(NormalizeSkill)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToHashSet();

        foreach (var target in targetSkills)
        {
            var normTarget = NormalizeSkill(target);
            var isMatched = false;

            if (candidateNormSet.Contains(normTarget))
            {
                isMatched = true;
            }
            else
            {
                // Substring check for composite skills e.g. "c#" in "c# backend"
                foreach (var cNorm in candidateNormSet)
                {
                    if (cNorm == normTarget || (normTarget.Length > 2 && cNorm.Contains(normTarget)))
                    {
                        isMatched = true;
                        break;
                    }
                }
            }

            if (isMatched)
                matched.Add(target);
            else
                missing.Add(target);
        }

        return (matched, missing);
    }

    public DeterministicScoreResult ComputeScore(
        List<string> candidateSkills,
        double candidateExperienceYears,
        List<string> mandatorySkills,
        List<string> optionalSkills,
        double minExperienceYears = 0.0,
        double mandatoryWeight = 70.0,
        double optionalWeight = 20.0,
        double experienceWeight = 10.0)
    {
        // 1. Mandatory Skills Computation (70% weight default)
        var (mandatoryMatched, mandatoryMissing) = CalculateSkillOverlap(candidateSkills, mandatorySkills);
        double mandatoryPoints;
        double mandatoryRatio;

        if (mandatorySkills != null && mandatorySkills.Count > 0)
        {
            mandatoryRatio = (double)mandatoryMatched.Count / mandatorySkills.Count;
            mandatoryPoints = mandatoryRatio * mandatoryWeight;
        }
        else
        {
            mandatoryRatio = 1.0;
            mandatoryPoints = mandatoryWeight;
        }

        // Guardrail: 25% penalty if missing >= 50% of mandatory skills
        var penaltyMultiplier = 1.0;
        if (mandatorySkills != null && mandatorySkills.Count > 0 && mandatoryRatio < 0.5)
        {
            penaltyMultiplier = 0.75;
        }

        // 2. Optional Skills Computation (20% weight default)
        var (optionalMatched, _) = CalculateSkillOverlap(candidateSkills, optionalSkills);
        double optionalPoints;

        if (optionalSkills != null && optionalSkills.Count > 0)
        {
            var optionalRatio = (double)optionalMatched.Count / optionalSkills.Count;
            optionalPoints = optionalRatio * optionalWeight;
        }
        else
        {
            optionalPoints = optionalWeight;
        }

        // 3. Experience Alignment Computation (10% weight default)
        double experiencePoints;
        if (minExperienceYears <= 0.0)
        {
            experiencePoints = experienceWeight;
        }
        else if (candidateExperienceYears >= minExperienceYears)
        {
            experiencePoints = experienceWeight;
        }
        else
        {
            var expRatio = Math.Max(0.0, candidateExperienceYears / minExperienceYears);
            experiencePoints = expRatio * experienceWeight;
        }

        // Total score calculation
        var rawScore = (mandatoryPoints + optionalPoints + experiencePoints) * penaltyMultiplier;
        var finalScore = Math.Round(Math.Clamp(rawScore, 0.0, 100.0), 1);

        return new DeterministicScoreResult
        {
            ConfidenceScore = finalScore,
            ScoreBreakdown = new ScoreBreakdown
            {
                MandatoryMatched = mandatoryMatched,
                MandatoryMissing = mandatoryMissing,
                OptionalMatched = optionalMatched
            },
            PointsDetail = new PointsDetail
            {
                MandatoryPoints = Math.Round(mandatoryPoints, 1),
                OptionalPoints = Math.Round(optionalPoints, 1),
                ExperiencePoints = Math.Round(experiencePoints, 1),
                PenaltyApplied = penaltyMultiplier < 1.0
            }
        };
    }
}
