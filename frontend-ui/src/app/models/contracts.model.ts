/**
 * RelAte Shared Data Contracts (TypeScript Interfaces)
 * Frozen on Day 1: M0 Contract Freeze
 */

export interface ParsedJD {
  role_title: string;
  mandatory_skills: string[];
  optional_skills: string[];
  min_experience_years: number;
}

export interface ScoreBreakdown {
  mandatory_matched: string[];
  mandatory_missing: string[];
  optional_matched: string[];
}

export interface PointsDetail {
  mandatory_points: number;
  optional_points: number;
  experience_points: number;
  penalty_applied: boolean;
}

export interface CandidateMatchResponse {
  candidate_id: string;
  name: string;
  linkedin_url: string;
  total_experience_years: number;
  confidence_score: number;
  score_breakdown: ScoreBreakdown;
  evidence_snippets: string[];
  fit_summary: string;
}

export interface RawJDRequest {
  raw_text: string;
}
