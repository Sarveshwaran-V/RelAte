using RelAte.Api.Models;

namespace RelAte.Api.Services;

public interface IDeterministicScoringEngine
{
    DeterministicScoreResult ComputeScore(
        List<string> candidateSkills,
        double candidateExperienceYears,
        List<string> mandatorySkills,
        List<string> optionalSkills,
        double minExperienceYears = 0.0,
        double mandatoryWeight = 70.0,
        double optionalWeight = 20.0,
        double experienceWeight = 10.0);

    (List<string> Matched, List<string> Missing) CalculateSkillOverlap(
        List<string> candidateSkills,
        List<string> targetSkills);

    string NormalizeSkill(string skill);
}
