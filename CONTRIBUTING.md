# RelAte Contribution & Team Git Workflow

To ensure seamless parallel development across all 5 tracks without blocking one another, follow these rules:

## 1. Branching Strategy

- **`main`**: Protected branch. Always deployable and passing tests.
- Feature branches are strictly track-prefixed:
  - Track 1 (Sarvesh): `feat/track-1-db-scoring`
  - Track 2A (Sivaram): `feat/track-2a-resume-extractor`
  - Track 2B (Pradeep): `feat/track-2b-skill-normalizer`
  - Track 3 (Priya): `feat/track-3-jd-retrieval`
  - Track 4 (Pavanesh): `feat/track-4-recruiter-ui`

## 2. Shared Data Contract Guardrail

The files under `/contracts` are **frozen on Day 1**. 
- No track may modify `parsed_jd.schema.json` or `candidate_match.schema.json` independently.
- Any contract change requires unanimous agreement and sign-off from Sarvesh and Priya/Pavanesh.

## 3. Pull Request Guidelines

1. Ensure unit tests pass before opening a PR:
   ```bash
   pytest backend-api/tests
   ```
2. Keep PRs small and scoped to your track deliverables.
3. Every PR must have at least one approval (Sarvesh or track peer) before merging into `main`.
