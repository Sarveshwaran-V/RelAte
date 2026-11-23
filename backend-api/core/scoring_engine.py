"""
Deterministic Confidence Scoring Engine
Strictly JD-Scoped, Mathematically Explainable & Reproducible
Owner: Sarvesh (Track 1)
"""
from typing import List, Set, Dict, Any, Tuple
import re


def normalize_skill(skill: str) -> str:
    """Normalize skill string for robust deterministic comparison."""
    cleaned = skill.strip().lower()
    # Normalize common variations (.net core -> dotnet core, etc.)
    cleaned = re.sub(r'[\s\-_]+', ' ', cleaned)
    return cleaned


def calculate_skill_overlap(candidate_skills: List[str], target_skills: List[str]) -> Tuple[List[str], List[str]]:
    """
    Deterministically computes matched and missing skills between candidate and target list.
    Preserves original target skill casing for UI display.
    """
    if not target_skills:
        return [], []

    candidate_norm_map = {normalize_skill(s): s for s in candidate_skills}
    candidate_norm_set = set(candidate_norm_map.keys())

    matched = []
    missing = []

    for target in target_skills:
        norm_target = normalize_skill(target)
        # Exact match or substring containment for composite titles (e.g. 'c#' in 'c# .net')
        matched_found = False
        if norm_target in candidate_norm_set:
            matched_found = True
        else:
            for c_norm in candidate_norm_set:
                if norm_target == c_norm or (len(norm_target) > 2 and norm_target in c_norm):
                    matched_found = True
                    break

        if matched_found:
            matched.append(target)
        else:
            missing.append(target)

    return matched, missing


def compute_deterministic_score(
    candidate_skills: List[str],
    candidate_experience_years: float,
    mandatory_skills: List[str],
    optional_skills: List[str],
    min_experience_years: float = 0.0,
    mandatory_weight: float = 70.0,
    optional_weight: float = 20.0,
    experience_weight: float = 10.0
) -> Dict[str, Any]:
    """
    Computes deterministic confidence score (0-100) strictly scoped to the JD.

    Breakdown:
      - Mandatory Skills Match (70%): Critical core technical requirements.
      - Optional Skills Match (20%): Secondary/nice-to-have capabilities.
      - Experience Alignment (10%): Proportional tenure match against minimum required.

    Returns:
      {
        "confidence_score": float,
        "score_breakdown": {
            "mandatory_matched": [...],
            "mandatory_missing": [...],
            "optional_matched": [...]
        },
        "points_detail": {
            "mandatory_points": float,
            "optional_points": float,
            "experience_points": float
        }
      }
    """
    # 1. Mandatory Skills Computation (70% default)
    mandatory_matched, mandatory_missing = calculate_skill_overlap(candidate_skills, mandatory_skills)
    if mandatory_skills:
        mandatory_ratio = len(mandatory_matched) / len(mandatory_skills)
        mandatory_points = mandatory_ratio * mandatory_weight
    else:
        # If no mandatory skills specified, full mandatory weight is granted
        mandatory_points = mandatory_weight
        mandatory_ratio = 1.0

    # Guardrail: Heavy penalty if missing more than half the mandatory skills
    penalty_multiplier = 1.0
    if mandatory_skills and mandatory_ratio < 0.5:
        penalty_multiplier = 0.75  # 25% penalty on total score

    # 2. Optional Skills Computation (20% default)
    optional_matched, _ = calculate_skill_overlap(candidate_skills, optional_skills)
    if optional_skills:
        optional_ratio = len(optional_matched) / len(optional_skills)
        optional_points = optional_ratio * optional_weight
    else:
        # If no optional skills defined, full optional credit
        optional_points = optional_weight

    # 3. Experience Alignment Computation (10% default)
    if min_experience_years <= 0:
        experience_points = experience_weight
    elif candidate_experience_years >= min_experience_years:
        experience_points = experience_weight
    else:
        # Proportional partial credit for experience
        exp_ratio = max(0.0, candidate_experience_years / min_experience_years)
        experience_points = exp_ratio * experience_weight

    # Total Sum
    raw_score = (mandatory_points + optional_points + experience_points) * penalty_multiplier
    final_score = round(max(0.0, min(100.0, raw_score)), 1)

    return {
        "confidence_score": final_score,
        "score_breakdown": {
            "mandatory_matched": mandatory_matched,
            "mandatory_missing": mandatory_missing,
            "optional_matched": optional_matched
        },
        "points_detail": {
            "mandatory_points": round(mandatory_points, 1),
            "optional_points": round(optional_points, 1),
            "experience_points": round(experience_points, 1),
            "penalty_applied": penalty_multiplier < 1.0
        }
    }
