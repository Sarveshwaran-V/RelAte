# RelAte: Internal Talent Pool Retrieval & Re-Engagement Engine

> **Unlocking WhiteBlue's 10,000+ candidate repository with hybrid retrieval and explainable deterministic skill scoring.**

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
3. **Two-Phase Model Strategy:** Build and validate rapidly using **Gemini Flash** (free tier), wrapped behind a clean provider abstraction layer so we can switch seamlessly to **Azure OpenAI** for production.
4. **Azure SQL Backbone:** Structured candidate metadata and vector embeddings live together in **Azure SQL** for unified hybrid querying.

---

## 3. Monorepo Structure

```
RelAte/
├── contracts/                  # Frozen JSON schemas and samples (Day 1 lock)
│   ├── parsed_jd.schema.json
│   ├── parsed_jd_sample.json
│   ├── candidate_match.schema.json
│   ├── candidate_match_sample.json
│   ├── contracts.py            # Pydantic v2 models
│   └── contracts.ts            # TypeScript definitions
│
├── database/                   # Azure SQL DDL scripts & hybrid search
│   ├── schema.sql              # Tables: candidates, candidate_skills, candidate_embeddings (VECTOR)
│   ├── hybrid_retrieval.sql    # Tier 1 hybrid search stored procedure
│   └── seed_mock.sql           # Development seed data
│
├── backend-api/                # FastAPI Application & Scorer
│   ├── main.py                 # Endpoints: /api/parse-jd, /api/match-candidates, /api/score
│   ├── core/
│   │   ├── config.py           # Environment settings
│   │   ├── llm_provider.py     # Gemini Flash & Azure OpenAI abstraction
│   │   └── scoring_engine.py   # Deterministic confidence scorer
│   └── tests/
│       └── test_scoring_engine.py
│
├── ingestion-worker/           # Resume Extraction & Skill Normalization
│   ├── extractor.py            # Sivaram: PDF/DOCX text & metadata extraction
│   ├── normalizer.py           # Pradeep: LLM canonical skill standardization
│   ├── embedder.py             # Pradeep: Vector embedding & Azure SQL batch upsert
│   └── golden_samples/         # 50 Golden Sample resumes placeholder
│
└── frontend-ui/                # Recruiter Dashboard Web App
    ├── index.html              # Recruiter dashboard with interactive criteria editor
    ├── styles.css              # Modern responsive styling
    ├── app.js                  # Frontend logic & API integration
    ├── mock_data.js            # Offline mock data adhering to contracts
    └── package.json
```

---

## 4. Team Task Split & Ownership

| Track | Lead | Module | Day 1 Deliverable |
| :--- | :--- | :--- | :--- |
| **Track 1** | **Sarvesh** | Architecture, Database & Deterministic Scorer | Monorepo scaffold, Azure SQL schema DDL, LLM abstraction, deterministic scorer + unit tests. |
| **Track 2A** | **Sivaram** | Resume Parsing & Extraction | PDF/DOCX extractor for 50-resume golden sample folder. |
| **Track 2B** | **Pradeep** | Skill Normalization & Bulk Ingestion | LLM normalizer prompt, embedding generator, batch upsert worker. |
| **Track 3** | **Priya** | JD Parser & Hybrid Retrieval API | `/api/parse-jd` endpoint and Tier 1 hybrid SQL query. |
| **Track 4** | **Pavanesh** | Recruiter Shortlist UI & Export | Standalone UI connected to mock contracts with CSV export & clipboard outreach tracker. |

---

## 5. Quick Start for Team Members

### Prerequisites
- Python 3.10+
- Node.js 18+ (optional, for frontend dev server)

### 1. Clone the Repository
```bash
git clone https://github.com/Sarveshwaran-V/RelAte.git
cd RelAte
```

### 2. Configure Environment
```bash
cp .env.example .env
# Edit .env and insert your GEMINI_API_KEY
```

### 3. Run Backend API & Tests
```bash
cd backend-api
pip install -r requirements.txt
pytest tests/ -v
python main.py
# API runs at http://localhost:8000 (Docs: http://localhost:8000/docs)
```

### 4. Launch Recruiter UI
```bash
cd frontend-ui
# Simply open index.html in any browser, or:
python -m http.server 3000
# Open http://localhost:3000
```

---

## 6. Execution Milestones

| Milestone | Deliverable |
| :--- | :--- |
| **M0: Contract Freeze** | Repo live, Azure SQL schema & JSON contracts locked, all 5 members coding in parallel. |
| **M1: UI + JD Parser Hookup** | Pavanesh connects UI criteria editor to Priya's live `/api/parse-jd` endpoint. |
| **M2: 50-Resume End-to-End** | Sivaram & Pradeep seed 50 Golden Sample resumes; Priya, Sarvesh & Pavanesh verify end-to-end pipeline. |
| **M3: 10,000+ Scale & TA Review** | Full batch ingestion completed; confidence scoring calibrated with Soumya and Vishnu. |
| **M4: Production Cutover** | Switch LLM provider config to Azure OpenAI and deploy for live TA usage. |
