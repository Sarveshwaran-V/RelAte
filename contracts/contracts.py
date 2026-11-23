"""
RelAte Shared Data Contracts (Pydantic v2 Models)
Frozen on Day 1: M0 Contract Freeze
"""
from typing import List
from uuid import UUID
from pydantic import BaseModel, Field, HttpUrl


class ParsedJD(BaseModel):
    """Contract A: Output from Priya's JD Parser consumed by Hybrid Retrieval & Scoring."""
    role_title: str = Field(..., description="Role title extracted from JD")
    mandatory_skills: List[str] = Field(default_factory=list, description="Mandatory non-negotiable skills")
    optional_skills: List[str] = Field(default_factory=list, description="Optional / nice-to-have skills")
    min_experience_years: float = Field(0.0, ge=0.0, description="Minimum years of experience required")


class ScoreBreakdown(BaseModel):
    """Detailed deterministic breakdown of skill matches."""
    mandatory_matched: List[str] = Field(default_factory=list)
    mandatory_missing: List[str] = Field(default_factory=list)
    optional_matched: List[str] = Field(default_factory=list)


class CandidateMatchResponse(BaseModel):
    """Contract B: Ranked candidate match output from Backend/Scorer consumed by Pavanesh's UI."""
    candidate_id: str = Field(..., description="Unique UUID identifying candidate")
    name: str = Field(..., description="Candidate full name")
    linkedin_url: str = Field(..., description="Candidate LinkedIn URL")
    total_experience_years: float = Field(..., ge=0.0, description="Candidate total years of experience")
    confidence_score: float = Field(..., ge=0.0, le=100.0, description="Deterministic confidence score (0-100)")
    score_breakdown: ScoreBreakdown = Field(..., description="Detailed breakdown of matched and missing skills")
    evidence_snippets: List[str] = Field(default_factory=list, description="Verbatim resume snippets evidencing skills")
    fit_summary: str = Field(..., description="1-2 sentence summary of candidate fit")
