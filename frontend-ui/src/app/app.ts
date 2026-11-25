import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TalentService } from './services/talent.service';
import { ParsedJD, CandidateMatchResponse } from './models/contracts.model';
import { MOCK_PARSED_JD, MOCK_CANDIDATES } from './models/mock-data';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private talentService = inject(TalentService);

  // Application State Signals
  dataMode = signal<'mock' | 'live'>('mock');
  isLoading = signal<boolean>(false);
  isSearching = signal<boolean>(false);

  sampleJdText = `Job Title: Senior .NET Backend Engineer
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

  rawJdInput = signal<string>(this.sampleJdText);
  parsedJd = signal<ParsedJD>({ ...MOCK_PARSED_JD });
  candidates = signal<CandidateMatchResponse[]>([...MOCK_CANDIDATES]);

  newMandatorySkill = signal<string>('');
  newOptionalSkill = signal<string>('');

  loadSampleJD() {
    this.rawJdInput.set(this.sampleJdText);
  }

  parseJobDescription() {
    this.isLoading.set(true);
    this.talentService.parseJobDescription(this.rawJdInput(), this.dataMode()).subscribe({
      next: (result) => {
        this.parsedJd.set(result);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  addMandatorySkill() {
    const skill = this.newMandatorySkill().trim();
    if (skill && !this.parsedJd().mandatory_skills.includes(skill)) {
      const current = this.parsedJd();
      this.parsedJd.set({
        ...current,
        mandatory_skills: [...current.mandatory_skills, skill]
      });
      this.newMandatorySkill.set('');
    }
  }

  removeMandatorySkill(index: number) {
    const current = this.parsedJd();
    const updated = [...current.mandatory_skills];
    updated.splice(index, 1);
    this.parsedJd.set({ ...current, mandatory_skills: updated });
  }

  addOptionalSkill() {
    const skill = this.newOptionalSkill().trim();
    if (skill && !this.parsedJd().optional_skills.includes(skill)) {
      const current = this.parsedJd();
      this.parsedJd.set({
        ...current,
        optional_skills: [...current.optional_skills, skill]
      });
      this.newOptionalSkill.set('');
    }
  }

  removeOptionalSkill(index: number) {
    const current = this.parsedJd();
    const updated = [...current.optional_skills];
    updated.splice(index, 1);
    this.parsedJd.set({ ...current, optional_skills: updated });
  }

  updateMinExperience(value: number) {
    const current = this.parsedJd();
    this.parsedJd.set({ ...current, min_experience_years: value });
  }

  retrieveCandidates() {
    this.isSearching.set(true);
    this.talentService.matchCandidates(this.parsedJd(), this.dataMode()).subscribe({
      next: (data) => {
        this.candidates.set(data);
        this.isSearching.set(false);
      },
      error: () => this.isSearching.set(false)
    });
  }

  exportCSV() {
    const headers = ['Rank', 'Name', 'Confidence Score', 'Total Exp (Years)', 'LinkedIn URL', 'Fit Summary'];
    const rows = this.candidates().map((c, i) => [
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
  }

  copyOutreachTracker() {
    const textLines = this.candidates().map((c, i) => 
      `${i + 1}. ${c.name} (${c.confidence_score}% fit, ${c.total_experience_years}y exp) - ${c.linkedin_url}\n   Summary: ${c.fit_summary}`
    ).join('\n\n');

    navigator.clipboard.writeText(textLines).then(() => {
      alert('Outreach candidate list copied to clipboard!');
    });
  }
}
