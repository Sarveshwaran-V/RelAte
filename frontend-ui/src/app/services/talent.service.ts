import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ParsedJD, CandidateMatchResponse, RawJDRequest } from '../models/contracts.model';
import { MOCK_PARSED_JD, MOCK_CANDIDATES } from '../models/mock-data';

@Injectable({
  providedIn: 'root'
})
export class TalentService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000'; // ASP.NET Core API default

  parseJobDescription(rawText: string, mode: 'mock' | 'live'): Observable<ParsedJD> {
    if (mode === 'mock') {
      return of({ ...MOCK_PARSED_JD });
    }

    const payload: RawJDRequest = { raw_text: rawText };
    return this.http.post<ParsedJD>(`${this.apiUrl}/api/parse-jd`, payload).pipe(
      catchError(err => {
        console.warn('API connection failed, falling back to mock data:', err);
        return of({ ...MOCK_PARSED_JD });
      })
    );
  }

  matchCandidates(jd: ParsedJD, mode: 'mock' | 'live'): Observable<CandidateMatchResponse[]> {
    if (mode === 'mock') {
      return of([...MOCK_CANDIDATES]);
    }

    return this.http.post<CandidateMatchResponse[]>(`${this.apiUrl}/api/match-candidates`, jd).pipe(
      catchError(err => {
        console.warn('API connection failed, falling back to mock candidates:', err);
        return of([...MOCK_CANDIDATES]);
      })
    );
  }
}
