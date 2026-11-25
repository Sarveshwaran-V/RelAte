# Ingestion Worker (.NET 10)

**Tracks 2A (Sivaram) & 2B (Pradeep)**

This module is a .NET 10 console pipeline for bulk processing raw resumes, extracting contact metadata, standardizing canonical skills with LLM, generating embeddings, and upserting records into Azure SQL.

## Architecture & Responsibilities

- **Track 2A (Sivaram - `ResumeExtractor.cs`):**
  - Text extraction from `.pdf` documents using `PdfPig`.
  - Text extraction from `.docx` documents using `DocumentFormat.OpenXml`.
  - Plain text extraction (`.txt`).
  - Metadata parsing: Full Name, Email, Phone, and LinkedIn profile URLs.
  - SHA-256 deduplication hashing to skip re-indexing identical resumes.

- **Track 2B (Pradeep - `SkillNormalizer.cs` & `VectorEmbedder.cs`):**
  - **`SkillNormalizer.cs`:** Invokes LLM (Gemini Flash / Azure OpenAI) to standardize variations (e.g. *"ASP.NET Core 8"*, *"C# developer"*) into canonical skill tags with exponential backoff & rate-limit handling.
  - **`VectorEmbedder.cs`:** Generates 1536-dimensional semantic embeddings from candidate role and skills.
  - **Azure SQL Upsert:** Batches candidates, normalized skills, and native `VECTOR(1536, FLOAT32)` embeddings into `dbo.candidates`, `dbo.candidate_skills`, and `dbo.candidate_embeddings` using `Microsoft.Data.SqlClient`.

## Quick Start

1. Place raw resumes (`.pdf`, `.docx`, `.txt`) into `golden_samples/`.
2. Build and run the ingestion worker:
   ```bash
   cd RelAte.Ingestion
   dotnet run
   ```
