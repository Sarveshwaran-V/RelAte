"""
Skill Normalization Pipeline
Owner: Pradeep (Track 2B)
Standardizes variations like 'ASP.NET WebAPI', '.NET Core', and 'C# backend' into canonical skill tags.
Includes backoff and rate-limiting to respect Gemini Flash free-tier quotas.
"""
import time
import json
from typing import List, Dict, Any
import logging

logger = logging.getLogger(__name__)

CANONICAL_SKILL_PROMPT = """
You are a technical taxonomy and resume skill normalization assistant.
Given the extracted resume text below, extract:
1. Candidate Full Name
2. Total Years of Experience (float)
3. Current or Last Job Role
4. List of canonical standardized skills (e.g. normalize 'ASP.NET Core 6' -> '.NET Core', 'C# developer' -> 'C#', 'SQL Server 2019' -> 'SQL Server').

Output valid JSON with keys:
{
  "full_name": string,
  "total_experience_years": number,
  "current_role": string,
  "skills": [
    {"canonical_skill": string, "raw_mention": string, "years_experience": number or null}
  ]
}
"""


def normalize_candidate_skills(raw_resume_text: str, llm_provider) -> Dict[str, Any]:
    """
    Invokes LLM with retry / exponential backoff for rate limits.
    """
    max_retries = 3
    delay = 2.0

    for attempt in range(max_retries):
        try:
            prompt = f"Resume Content:\n\n{raw_resume_text[:4000]}"
            result = llm_provider.generate_structured(prompt, system_instruction=CANONICAL_SKILL_PROMPT)
            return result
        except Exception as e:
            logger.warning(f"Attempt {attempt + 1} failed: {e}")
            if attempt < max_retries - 1:
                time.sleep(delay)
                delay *= 2
            else:
                raise e


if __name__ == "__main__":
    print("Pradeep's Skill Normalizer Module initialized.")
