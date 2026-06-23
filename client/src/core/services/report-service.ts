import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { CreateReport, ModerationReport } from '../../types/moderation';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  createReport(report: CreateReport) {
    return this.http.post<ModerationReport>(this.baseUrl + 'reports', report);
  }
}
