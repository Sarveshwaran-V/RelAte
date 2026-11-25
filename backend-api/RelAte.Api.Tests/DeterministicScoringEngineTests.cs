using RelAte.Api.Services;
using Xunit;

namespace RelAte.Api.Tests;

public class DeterministicScoringEngineTests
{
    private readonly DeterministicScoringEngine _engine = new();

    [Fact]
    public void TestPerfectMatch()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "C#", ".NET Core", "SQL Server", "Azure", "Docker" },
            candidateExperienceYears: 5.0,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string> { "Azure", "Docker" },
            minExperienceYears: 3.0
        );

        Assert.Equal(100.0, result.ConfidenceScore);
        Assert.Equal(3, result.ScoreBreakdown.MandatoryMatched.Count);
        Assert.Empty(result.ScoreBreakdown.MandatoryMissing);
        Assert.Equal(2, result.ScoreBreakdown.OptionalMatched.Count);
        Assert.False(result.PointsDetail.PenaltyApplied);
    }

    [Fact]
    public void TestPartialMatchContractBScenario()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "C#", ".NET Core", "Azure", "REST APIs" },
            candidateExperienceYears: 4.5,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string> { "Azure", "Docker" },
            minExperienceYears: 3.0
        );

        // Mandatory: 2/3 * 70 = 46.67
        // Optional: 1/2 * 20 = 10.0
        // Experience: 4.5 >= 3.0 -> 10.0
        // Total ~ 66.7
        Assert.InRange(result.ConfidenceScore, 66.0, 67.0);
        Assert.Contains("C#", result.ScoreBreakdown.MandatoryMatched);
        Assert.Contains(".NET Core", result.ScoreBreakdown.MandatoryMatched);
        Assert.Contains("SQL Server", result.ScoreBreakdown.MandatoryMissing);
        Assert.Contains("Azure", result.ScoreBreakdown.OptionalMatched);
    }

    [Fact]
    public void TestGuardrailPenaltyWhenMissingMajorityMandatory()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "Docker", "Azure" },
            candidateExperienceYears: 5.0,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string> { "Azure", "Docker" },
            minExperienceYears: 3.0
        );

        // Raw = 0 (mandatory) + 20 (optional) + 10 (exp) = 30. Penalty 0.75x -> 22.5
        Assert.True(result.PointsDetail.PenaltyApplied);
        Assert.Equal(22.5, result.ConfidenceScore);
        Assert.Equal(3, result.ScoreBreakdown.MandatoryMissing.Count);
    }

    [Fact]
    public void TestCaseAndWhitespaceInsensitivity()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "  c# ", ".net core", "sql server " },
            candidateExperienceYears: 4.0,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string>(),
            minExperienceYears: 2.0
        );

        Assert.Equal(100.0, result.ConfidenceScore);
        Assert.Empty(result.ScoreBreakdown.MandatoryMissing);
    }

    [Fact]
    public void TestExperienceProportionalScaling()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            candidateExperienceYears: 1.5,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string>(),
            minExperienceYears: 3.0
        );

        // Mandatory: 70
        // Optional: 20
        // Exp: (1.5/3.0) * 10 = 5.0
        // Total = 95.0
        Assert.Equal(95.0, result.ConfidenceScore);
        Assert.Equal(5.0, result.PointsDetail.ExperiencePoints);
    }

    [Fact]
    public void TestZeroMatch()
    {
        var result = _engine.ComputeScore(
            candidateSkills: new List<string> { "Photoshop", "Graphic Design" },
            candidateExperienceYears: 0.0,
            mandatorySkills: new List<string> { "C#", ".NET Core", "SQL Server" },
            optionalSkills: new List<string> { "Azure" },
            minExperienceYears: 3.0
        );

        Assert.Equal(0.0, result.ConfidenceScore);
        Assert.Equal(3, result.ScoreBreakdown.MandatoryMissing.Count);
    }
}
