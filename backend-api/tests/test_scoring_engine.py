"""
Unit tests for RelAte Deterministic Scoring Engine
Verifies mathematical reproducibility, guardrails, and contract breakdown
"""
import pytest
import sys
import os

# Ensure backend-api is on sys.path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

from core.scoring_engine import compute_deterministic_score, normalize_skill, calculate_skill_overlap


def test_perfect_match():
    """A candidate with 100% mandatory skills, 100% optional skills and adequate experience scores 100.0."""
    result = compute_deterministic_score(
        candidate_skills=["C#", ".NET Core", "SQL Server", "Azure", "Docker"],
        candidate_experience_years=5.0,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=["Azure", "Docker"],
        min_experience_years=3.0
    )
    assert result["confidence_score"] == 100.0
    assert result["score_breakdown"]["mandatory_matched"] == ["C#", ".NET Core", "SQL Server"]
    assert result["score_breakdown"]["mandatory_missing"] == []
    assert result["score_breakdown"]["optional_matched"] == ["Azure", "Docker"]
    assert not result["points_detail"]["penalty_applied"]


def test_partial_match_contract_b_scenario():
    """Verify standard partial match matching Contract B profile."""
    result = compute_deterministic_score(
        candidate_skills=["C#", ".NET Core", "Azure", "REST APIs"],
        candidate_experience_years=4.5,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=["Azure", "Docker"],
        min_experience_years=3.0
    )
    # Mandatory matched: 2/3 (66.7%), points: 46.7
    # Optional matched: 1/2 (50%), points: 10.0
    # Experience: 4.5 >= 3.0 -> 10.0
    # Expected sum: ~66.7
    assert 66.0 <= result["confidence_score"] <= 67.0
    assert result["score_breakdown"]["mandatory_matched"] == ["C#", ".NET Core"]
    assert result["score_breakdown"]["mandatory_missing"] == ["SQL Server"]
    assert result["score_breakdown"]["optional_matched"] == ["Azure"]


def test_guardrail_penalty_when_missing_majority_mandatory():
    """Candidates missing >=50% of mandatory skills must receive a penalty multiplier."""
    result = compute_deterministic_score(
        candidate_skills=["Docker", "Azure"],  # Has optional skills, but only 0 mandatory skills
        candidate_experience_years=5.0,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=["Azure", "Docker"],
        min_experience_years=3.0
    )
    # Mandatory: 0/3 = 0 pts
    # Optional: 2/2 = 20 pts
    # Exp: 10 pts
    # Raw = 30 pts. Penalty 0.75x -> 22.5 pts.
    assert result["points_detail"]["penalty_applied"] is True
    assert result["confidence_score"] == 22.5
    assert len(result["score_breakdown"]["mandatory_missing"]) == 3


def test_case_and_whitespace_insensitivity():
    """Skill matching must be resilient to casing, trailing whitespace, and minor variations."""
    result = compute_deterministic_score(
        candidate_skills=["  c# ", ".net core", "sql server "],
        candidate_experience_years=4.0,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=[],
        min_experience_years=2.0
    )
    assert result["confidence_score"] == 100.0
    assert len(result["score_breakdown"]["mandatory_missing"]) == 0


def test_experience_proportional_scaling():
    """Candidates below minimum experience receive linearly scaled partial experience credit."""
    result = compute_deterministic_score(
        candidate_skills=["C#", ".NET Core", "SQL Server"],
        candidate_experience_years=1.5,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=[],
        min_experience_years=3.0
    )
    # Mandatory: 70 pts
    # Optional: 20 pts (empty list default)
    # Exp: (1.5/3.0) * 10 = 5.0 pts
    # Total: 95.0
    assert result["confidence_score"] == 95.0
    assert result["points_detail"]["experience_points"] == 5.0


def test_zero_match():
    """Candidate with completely unrelated skills receives 0 score for skills."""
    result = compute_deterministic_score(
        candidate_skills=["Photoshop", "Graphic Design"],
        candidate_experience_years=0.0,
        mandatory_skills=["C#", ".NET Core", "SQL Server"],
        optional_skills=["Azure"],
        min_experience_years=3.0
    )
    assert result["confidence_score"] == 0.0
    assert result["score_breakdown"]["mandatory_missing"] == ["C#", ".NET Core", "SQL Server"]
