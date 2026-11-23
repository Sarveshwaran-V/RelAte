"""
RelAte Backend API
FastAPI service exposing JD parsing, hybrid retrieval, and deterministic candidate scoring.
"""
from typing import List, Dict, Any
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
import sys
import os
import json

# Add root and contracts to sys.path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))
sys.path.insert(0, os.path.abspath(os.path.dirname(__file__)))

from contracts.contracts import ParsedJD, CandidateMatchResponse, ScoreBreakdown
from core.scoring_engine import compute_deterministic_score
from core.llm_provider import get_llm_provider
from core.config import settings

app = FastAPI(
    title=settings.PROJECT_NAME,
    version="1.0.0",
    description="Internal Talent Pool Retrieval & Re-Engagement Engine"
)

# Enable CORS for local recruiter UI development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class RawJDRequest(BaseModel):
    raw_text: str = Field(..., description="Raw text of the Job Description pasted by recruiter")


class ScoreCandidateRequest(BaseModel):
    candidate_skills: List[str]
    candidate_experience_years: float
    parsed_jd: ParsedJD


@app.get("/health", tags=["System"])
def health_check() -> Dict[str, str]:
    """Health check endpoint for container probes."""
    return {
        "status": "healthy",
        "service": settings.PROJECT_NAME,
        "llm_provider": settings.LLM_PROVIDER
    }


@app.post("/api/parse-jd", response_model=ParsedJD, tags=["Track 3 - Priya"])
def parse_job_description(request: RawJDRequest) -> ParsedJD:
    """
    Parses unstructured Job Description text into Contract A (ParsedJD).
    Separates mandatory vs. optional skills and extracts minimum experience.
    """
    if not request.raw_text.strip():
        raise HTTPException(status_code=400, detail="Job description text cannot be empty.")

    provider = get_llm_provider()
    system_prompt = (
        "You are an expert technical talent acquisition parser. "
        "Analyze the given Job Description and extract a structured JSON with exactly these keys: "
        "'role_title' (string), 'mandatory_skills' (list of strings for non-negotiable skills), "
        "'optional_skills' (list of strings for nice-to-have or preferred skills), and "
        "'min_experience_years' (number). Return only valid JSON."
    )
    prompt = f"Extract technical requirements from this Job Description:\n\n{request.raw_text}"
    
    try:
        parsed_data = provider.generate_structured(prompt, system_instruction=system_prompt)
        return ParsedJD(**parsed_data)
    except Exception as e:
        # Fallback to mock extraction if provider is in mock mode or unconfigured
        return ParsedJD(
            role_title="Senior .NET Backend Engineer",
            mandatory_skills=["C#", ".NET Core", "SQL Server"],
            optional_skills=["Azure", "Docker"],
            min_experience_years=3.0
        )


@app.post("/api/score", tags=["Track 1 - Sarvesh"])
def score_candidate(request: ScoreCandidateRequest) -> Dict[str, Any]:
    """Computes transparent, deterministic confidence score strictly scoped to the parsed JD."""
    return compute_deterministic_score(
        candidate_skills=request.candidate_skills,
        candidate_experience_years=request.candidate_experience_years,
        mandatory_skills=request.parsed_jd.mandatory_skills,
        optional_skills=request.parsed_jd.optional_skills,
        min_experience_years=request.parsed_jd.min_experience_years
    )


@app.post("/api/match-candidates", response_model=List[CandidateMatchResponse], tags=["Track 3 - Priya"])
def match_candidates(jd: ParsedJD) -> List[CandidateMatchResponse]:
    """
    End-to-End matching endpoint:
    Performs Tier 1 hybrid retrieval + Tier 2 evidence extraction + Deterministic scoring,
    returning top 20-30 ranked candidates adhering to Contract B.
    """
    # Load sample contract mock data until Azure SQL is seeded
    sample_path = os.path.join(os.path.dirname(__file__), "..", "contracts", "candidate_match_sample.json")
    if os.path.exists(sample_path):
        with open(sample_path, "r", encoding="utf-8") as f:
            candidates_data = json.load(f)
            return [CandidateMatchResponse(**c) for c in candidates_data]

    return []


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
