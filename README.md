# RelAte: Internal Talent Pool Retrieval & Re-Engagement Engine

> **Unlocking WhiteBlue's 10,000+ candidate repository with hybrid retrieval, deterministic skill scoring, .NET 10 Web API, and Angular.**

**Project Lead:** Sarveshwaran V (Sarvesh)  
**Mentor:** Vishnu  
**Stakeholder:** Soumya & WhiteBlue Talent Acquisition (TA) Team  
**Engineering Team:** Sarvesh, Sivaram, Pradeep, Priya, Pavanesh  

---

## 1. The Product Vision
**For** WhiteBlue’s Talent Acquisition (TA) team,  
**Who** need to fill open technical roles quickly without relying exclusively on expensive cold outbound sourcing,  
**This system** is an intelligent hybrid retrieval and candidate-ranking engine  
**That** unlocks our existing repository of **10,000+ previously interviewed resumes**, surfacing the top 20–30 skill-matched candidates with explainable confidence scores and updated LinkedIn profiles—enabling warm, high-response-rate re-engagement.

---

## 2. Core Architectural Principles
1. **AI Assists, Math Decides:** The LLM handles unstructured language tasks (parsing JDs, normalizing skill synonyms, extracting resume evidence, summarizing fit). **Deterministic application code** computes the final confidence score so every score is transparent, reproducible, and easy to explain to HR.
2. **Strictly JD-Scoped Scoring:** Confidence scores are driven purely by technical alignment with the JD (mandatory skills, optional skills, and overall skill relevance). External variables remain with the HR team.
3. **Two-Phase Model Strategy:** Build and validate rapidly using **Gemini Flash** (free tier), wrapped behind a clean provider abstraction layer (`ILlmProvider`) so we can switch seamlessly to **Azure OpenAI** for production.
4. **Azure SQL Backbone:** Structured candidate metadata and vector embeddings live together in **Azure SQL** for unified hybrid querying.
5. **Modern Tech Stack:** **.NET 10 / ASP.NET Core** backend and **Angular** recruiter portal for enterprise performance, strong typing, and rapid parallel team development.

---

## 3. Monorepo Structure

```
RelAte/
├── contracts/                          # Day 1 Frozen Data Contracts (Milestone M0)
│   ├── parsed_jd.schema.json           # Contract A JSON Schema (Priya -> Hybrid Retrieval & Scoring)
│   ├── parsed_jd_sample.json           # Mock JSON instance
│   ├── candidate_match.schema.json     # Contract B JSON Schema (Scorer -> Pavanesh's UI)
│   ├── candidate_match_sample.json     # Mock candidate shortlist
│   ├── Contracts.cs                    # C# DTO models
│   └── contracts.ts                    # TypeScript definitions
│
├── database/                           # Azure SQL Relational + Vector Storage (Sarvesh - Track 1)
│   ├── schema.sql                      # DDL for candidates, candidate_skills, and VECTOR(1536) table
│   ├── hybrid_retrieval.sql            # Tier 1 hybrid search (keyword + VECTOR_DISTANCE cosine)
│   └── seed_mock.sql                   # Realistic candidate seed records for testing
│
├── backend-api/                        # .NET 10 ASP.NET Core Solution (Sarvesh & Priya)
│   ├── RelAte.slnx                     # .NET Solution file
│   ├── RelAte.Api/
│   │   ├── Controllers/TalentController.cs # /health, /api/parse-jd, /api/match-candidates, /api/score
│   │   ├── Services/
│   │   │   ├── IDeterministicScoringEngine.cs & DeterministicScoringEngine.cs # Scorer (100% unit tested)
│   │   │   ├── ILlmProvider.cs & LlmProviders.cs # Gemini Flash & Azure OpenAI
│   │   └── Program.cs                  # DI, CORS, and endpoint routing
│   └── RelAte.Api.Tests/
│       └── DeterministicScoringEngineTests.cs # 6 xUnit tests validating mathematical scoring
│
├── ingestion-worker/                   # Resume Extraction & Skill Normalization (Sivaram & Pradeep)
│   ├── RelAte.Ingestion/               # .NET Ingestion Worker Console
│   │   ├── ResumeExtractor.cs          # Deduplication hashing & regex metadata extraction
│   │   └── Program.cs
│   ├── extractor.py                    # Python extractor pipeline
│   ├── normalizer.py                   # Canonical skill normalizer
│   ├── embedder.py                     # Vector generator & batch upsert worker
│   └── golden_samples/                 # 50 Golden Sample resumes placeholder
│
└── frontend-ui/                        # Angular Recruiter Shortlist UI (Pavanesh - Track 4)
    ├── src/app/
    │   ├── models/contracts.model.ts   # Contract A & B models
    │   ├── models/mock-data.ts         # Mock candidates & sample JD
    │   ├── services/talent.service.ts  # API & mock integration service
    │   ├── app.ts                      # Standalone component logic & signal state
    │   ├── app.html                    # Recruiter dashboard UI with chips and cards
    │   └── app.css                     # Responsive styling
    ├── angular.json
    └── package.json
```

---

## 4. Team Task Split & Ownership

| Track | Lead | Module | Day 1 Deliverable |
| :--- | :--- | :--- | :--- |
| **Track 1** | **Sarvesh** | Architecture, Database & Deterministic Scorer | Monorepo scaffold, Azure SQL schema DDL, C# LLM provider abstraction, C# deterministic scorer + xUnit tests. |
| **Track 2A** | **Sivaram** | Resume Parsing & Extraction | PDF/DOCX extractor for 50-resume golden sample folder. |
| **Track 2B** | **Pradeep** | Skill Normalization & Bulk Ingestion | LLM normalizer prompt, embedding generator, batch upsert worker. |
| **Track 3** | **Priya** | JD Parser & Hybrid Retrieval API | `/api/parse-jd` endpoint in ASP.NET Core and Tier 1 hybrid SQL query. |
| **Track 4** | **Pavanesh** | Recruiter Shortlist UI & Export | Standalone Angular UI connected to mock contracts with CSV export & clipboard outreach tracker. |

---

## 5. Quick Start for Team Members

### Prerequisites
- **.NET 8 or 10 SDK** (`dotnet --version`)
- **Node.js 18+ & Angular CLI** (`ng version`)

### 1. Clone the Repository
```bash
git clone https://github.com/Sarveshwaran-V/RelAte.git
cd RelAte
```

### 2. Run .NET Backend & Tests
```bash
cd backend-api
dotnet test RelAte.slnx
cd RelAte.Api
dotnet run
# API runs at http://localhost:5000 (Health probe: http://localhost:5000/health)
```

### 3. Run Angular Recruiter Dashboard
```bash
cd frontend-ui
npm install
npm start
# Open http://localhost:4200/
```

---

## 6. Execution Milestones

| Milestone | Deliverable |
| :--- | :--- |
| **M0: Contract Freeze** | Repo live, Azure SQL schema & JSON contracts locked, all 5 members coding in parallel. |
| **M1: UI + JD Parser Hookup** | Pavanesh connects Angular criteria editor to Priya's live `/api/parse-jd` endpoint. |
| **M2: 50-Resume End-to-End** | Sivaram & Pradeep seed 50 Golden Sample resumes; Priya, Sarvesh & Pavanesh verify end-to-end pipeline. |
| **M3: 10,000+ Scale & TA Review** | Full batch ingestion completed; confidence scoring calibrated with Soumya and Vishnu. |
| **M4: Production Cutover** | Switch LLM provider config to Azure OpenAI and deploy for live TA usage. |
