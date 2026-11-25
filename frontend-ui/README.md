# Recruiter Shortlist UI (Angular)

**Owner:** Pavanesh (Track 4)

This Angular application serves as the recruiter-facing portal to interactively review/edit parsed JD criteria and browse ranked candidate cards from WhiteBlue's 10,000+ internal talent pool.

## Features

- **JD Parser & Criteria Tuning:** Paste raw JD text, auto-parse with AI into Mandatory vs. Optional skills, and edit chips interactively.
- **Top Ranked Candidate Cards:** Displays confidence score badges, mandatory/optional breakdown tags, verbatim resume evidence snippets, fit summaries, and direct LinkedIn profile links.
- **One-Click Export & Outreach:** One-click CSV export and copy-to-clipboard for recruiter outreach tracking.
- **Dual Data Mode:** Toggle between **Local Mock** (zero dependency on backend) and **Live .NET API** (`http://localhost:5000`).

## Development Server

1. Install dependencies:
   ```bash
   npm install
   ```
2. Start the Angular development server:
   ```bash
   npm start
   # or
   ng serve
   ```
3. Navigate to `http://localhost:4200/`.
