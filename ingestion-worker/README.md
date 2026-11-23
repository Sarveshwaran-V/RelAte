# Ingestion Worker (Tracks 2A & 2B)

This module handles raw resume processing, LLM skill normalization, embedding generation, and bulk loading into Azure SQL.

## Team Ownership

- **Track 2A (Sivaram):** `extractor.py`
  - Extracts text from `.pdf` and `.docx` in `golden_samples/`.
  - Parses Phone, Email, LinkedIn URL, and file deduplication hash.
  - Passes intermediate JSON to Track 2B.

- **Track 2B (Pradeep):** `normalizer.py` & `embedder.py`
  - Prompts Gemini Flash to standardize skills into canonical tags.
  - Generates 1536-dim vector embeddings.
  - Upserts candidates into Azure SQL (`dbo.candidates`, `dbo.candidate_skills`, `dbo.candidate_embeddings`).

## Quick Start

1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
2. Place candidate resume files into `golden_samples/`.
3. Run extraction test:
   ```bash
   python extractor.py
   ```
