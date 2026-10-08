import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from './api.config';
import { ProcessResult, QuarantineRecord, UploadResult } from './models';

/** Único punto de acceso a la API REST. */
@Injectable({ providedIn: 'root' })
export class CommerceService {
  private readonly http = inject(HttpClient);

  upload(file: File): Observable<UploadResult> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<UploadResult>(`${API_URL}/upload`, form);
  }

  process(date: string): Observable<ProcessResult> {
    return this.http.post<ProcessResult>(`${API_URL}/process`, { date });
  }

  getQuarantine(): Observable<QuarantineRecord[]> {
    return this.http.get<QuarantineRecord[]>(`${API_URL}/quarantine`);
  }
}
