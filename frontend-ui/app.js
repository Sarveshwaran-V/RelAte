import { MOCK_PARSED_JD, MOCK_CANDIDATES } from './mock_data.js';

// Application State
let currentJD = { ...MOCK_PARSED_JD };
let currentCandidates = [...MOCK_CANDIDATES];

// DOM Elements
const jdTextInput = document.getElementById('jdTextInput');
const btnParseJD = document.getElementById('btnParseJD');
const btnLoadSampleJD = document.getElementById('btnLoadSampleJD');
const roleTitleInput = document.getElementById('roleTitleInput');
const mandatoryChips = document.getElementById('mandatoryChips');
const optionalChips = document.getElementById('optionalChips');
const newMandatoryInput = document.getElementById('newMandatoryInput');
const newOptionalInput = document.getElementById('newOptionalInput');
const btnAddMandatory = document.getElementById('btnAddMandatory');
const btnAddOptional = document.getElementById('btnAddOptional');
const minExpInput = document.getElementById('minExpInput');
const expValueDisplay = document.getElementById('expValueDisplay');
const btnSearchCandidates = document.getElementById('btnSearchCandidates');
const candidatesList = document.getElementById('candidatesList');
const matchCountText = document.getElementById('matchCountText');
const btnCopyTracker = document.getElementById('btnCopyTracker');
const btnExportCSV = document.getElementById('btnExportCSV');
const dataModeSelect = document.getElementById('dataModeSelect');

// Sample JD Text
const SAMPLE_JD_RAW = `Job Title: Senior .NET Backend Engineer
Location: Chennai / Remote (WhiteBlue)
Experience: 3-6 years

Key Responsibilities:
- Design and develop scalable backend APIs and microservices using C# and .NET Core.
- Write complex database queries, stored procedures, and optimize schema in Microsoft SQL Server.
- Build CI/CD deployment pipelines on Microsoft Azure.
- Containerize services with Docker.

Requirements:
- 3+ years experience with C# and .NET Core.
- Strong hands-on database experience in SQL Server.
- Nice to have: Azure cloud experience, Docker containerization.`;

function init() {
  jdTextInput.value = SAMPLE_JD_RAW;
  renderCriteria();
  renderCandidates();
  attachEventListeners();
}

function renderCriteria() {
  roleTitleInput.value = currentJD.role_title;
  minExpInput.value = currentJD.min_experience_years;
  expValueDisplay.textContent = `${currentJD.min_experience_years} Years`;

  // Render Mandatory Chips
  mandatoryChips.innerHTML = currentJD.mandatory_skills.map((skill, index) => `
    <span class="chip mandatory">
      ${skill}
      <span class="chip-remove" data-type="mandatory" data-index="${index}">×</span>
    </span>
  `).join('');

  // Render Optional Chips
  optionalChips.innerHTML = currentJD.optional_skills.map((skill, index) => `
    <span class="chip optional">
      ${skill}
      <span class="chip-remove" data-type="optional" data-index="${index}">×</span>
    </span>
  `).join('');
}

function renderCandidates() {
  matchCountText.textContent = `Showing ${currentCandidates.length} technical matches from WhiteBlue internal talent pool`;

  candidatesList.innerHTML = currentCandidates.map((c, index) => {
    const scoreClass = c.confidence_score >= 85 ? 'score-high' : 'score-med';
    const mandatoryMatchedPills = (c.score_breakdown.mandatory_matched || []).map(s => 
      `<span class="skill-pill matched">✓ ${s}</span>`
    ).join('');
    const mandatoryMissingPills = (c.score_breakdown.mandatory_missing || []).map(s => 
      `<span class="skill-pill missing">✕ Missing: ${s}</span>`
    ).join('');
    const optionalMatchedPills = (c.score_breakdown.optional_matched || []).map(s => 
      `<span class="skill-pill optional">+ ${s}</span>`
    ).join('');

    const evidenceHtml = (c.evidence_snippets || []).map(snippet => `
      <div class="evidence-box">"${snippet}"</div>
    `).join('');

    return `
      <article class="candidate-card">
        <div class="candidate-top">
          <div>
            <div class="candidate-name">
              <span>#${index + 1} ${c.name}</span>
              <a href="${c.linkedin_url}" target="_blank" rel="noopener noreferrer" class="linkedin-link">
                LinkedIn Profile ↗
              </a>
            </div>
            <p class="candidate-tenure">${c.total_experience_years} Years Total Experience</p>
          </div>
          <div class="score-badge ${scoreClass}">
            ${c.confidence_score}% Fit
          </div>
        </div>

        <div class="skill-pills-row">
          ${mandatoryMatchedPills}
          ${mandatoryMissingPills}
          ${optionalMatchedPills}
        </div>

        ${evidenceHtml}

        <p class="fit-summary"><strong>Fit Summary:</strong> ${c.fit_summary}</p>
      </article>
    `;
  }).join('');
}

function attachEventListeners() {
  btnLoadSampleJD.addEventListener('click', () => {
    jdTextInput.value = SAMPLE_JD_RAW;
  });

  minExpInput.addEventListener('input', (e) => {
    currentJD.min_experience_years = parseFloat(e.target.value);
    expValueDisplay.textContent = `${currentJD.min_experience_years} Years`;
  });

  // Add Mandatory Skill
  btnAddMandatory.addEventListener('click', () => {
    const val = newMandatoryInput.value.trim();
    if (val && !currentJD.mandatory_skills.includes(val)) {
      currentJD.mandatory_skills.push(val);
      newMandatoryInput.value = '';
      renderCriteria();
    }
  });

  // Add Optional Skill
  btnAddOptional.addEventListener('click', () => {
    const val = newOptionalInput.value.trim();
    if (val && !currentJD.optional_skills.includes(val)) {
      currentJD.optional_skills.push(val);
      newOptionalInput.value = '';
      renderCriteria();
    }
  });

  // Remove Chips
  document.addEventListener('click', (e) => {
    if (e.target.classList.contains('chip-remove')) {
      const type = e.target.getAttribute('data-type');
      const idx = parseInt(e.target.getAttribute('data-index'), 10);
      if (type === 'mandatory') {
        currentJD.mandatory_skills.splice(idx, 1);
      } else {
        currentJD.optional_skills.splice(idx, 1);
      }
      renderCriteria();
    }
  });

  // Parse JD Button
  btnParseJD.addEventListener('click', async () => {
    const mode = dataModeSelect.value;
    if (mode === 'live') {
      try {
        btnParseJD.disabled = true;
        btnParseJD.textContent = 'Parsing...';
        const res = await fetch('http://localhost:8000/api/parse-jd', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ raw_text: jdTextInput.value })
        });
        currentJD = await res.json();
        renderCriteria();
      } catch (err) {
        alert('Could not connect to live backend on localhost:8000. Using mock data.');
        currentJD = { ...MOCK_PARSED_JD };
        renderCriteria();
      } finally {
        btnParseJD.disabled = false;
        btnParseJD.textContent = '⚡ Parse with AI';
      }
    } else {
      // Mock Parse
      currentJD = { ...MOCK_PARSED_JD };
      renderCriteria();
    }
  });

  // Search Candidates Button
  btnSearchCandidates.addEventListener('click', async () => {
    const mode = dataModeSelect.value;
    if (mode === 'live') {
      try {
        btnSearchCandidates.disabled = true;
        btnSearchCandidates.textContent = 'Retrieving Candidates...';
        const res = await fetch('http://localhost:8000/api/match-candidates', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(currentJD)
        });
        currentCandidates = await res.json();
        renderCandidates();
      } catch (err) {
        alert('Could not connect to live backend. Displaying mock candidates.');
        currentCandidates = [...MOCK_CANDIDATES];
        renderCandidates();
      } finally {
        btnSearchCandidates.disabled = false;
        btnSearchCandidates.textContent = '🔍 Retrieve & Rank Talent Pool';
      }
    } else {
      currentCandidates = [...MOCK_CANDIDATES];
      renderCandidates();
    }
  });

  // CSV Export
  btnExportCSV.addEventListener('click', () => {
    const headers = ['Rank', 'Name', 'Confidence Score', 'Total Exp (Years)', 'LinkedIn URL', 'Fit Summary'];
    const rows = currentCandidates.map((c, i) => [
      i + 1,
      `"${c.name}"`,
      `${c.confidence_score}%`,
      c.total_experience_years,
      c.linkedin_url,
      `"${c.fit_summary.replace(/"/g, '""')}"`
    ]);

    const csvContent = "data:text/csv;charset=utf-8," + [headers.join(','), ...rows.map(e => e.join(','))].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `RelAte_Shortlist_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  });

  // Copy for Outreach Tracker
  btnCopyTracker.addEventListener('click', () => {
    const textLines = currentCandidates.map((c, i) => 
      `${i + 1}. ${c.name} (${c.confidence_score}% fit, ${c.total_experience_years}y exp) - ${c.linkedin_url}\n   Summary: ${c.fit_summary}`
    ).join('\n\n');

    navigator.clipboard.writeText(textLines).then(() => {
      alert('Outreach candidate summary copied to clipboard!');
    });
  });
}

document.addEventListener('DOMContentLoaded', init);
