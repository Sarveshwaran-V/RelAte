# Recruiter Shortlist UI (Track 4)

**Owner:** Pavanesh  

This web application serves as the recruiter-facing portal to interactively review/edit parsed JD criteria and browse ranked candidate cards from WhiteBlue's 10,000+ internal talent pool.

## Features

- **JD Parser & Criteria Tuning:** Paste raw JD text, auto-parse with AI into Mandatory vs. Optional skills, and edit chips interactively.
- **Top Ranked Candidate Cards:** Displays confidence score badges, mandatory/optional breakdown tags, verbatim resume evidence snippets, fit summaries, and direct LinkedIn profile links.
- **One-Click Export & Outreach:** One-click CSV export and copy-to-clipboard for recruiter outreach tracking.
- **Dual Data Mode:** Toggle between **Local Mock** (zero dependency on backend) and **Live Backend** (`http://localhost:8000`).

## How to Run

You can run the UI directly in any modern browser by double-clicking `index.html`, or launch a local server:
```bash
cd frontend-ui
npx serve -l 3000 .
# or
python -m http.server 3000
```
Then visit `http://localhost:3000`.
