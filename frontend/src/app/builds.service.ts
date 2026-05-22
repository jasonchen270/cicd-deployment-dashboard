import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Build, BuildSource, BuildStatus, RetriggerResult } from './build.model';

@Injectable({ providedIn: 'root' })
export class BuildsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl =
    (window as unknown as { __API_BASE__?: string }).__API_BASE__ ?? 'http://localhost:5085';

  list(filter?: { status?: BuildStatus; source?: BuildSource }): Observable<Build[]> {
    let params = new HttpParams();
    if (filter?.status) params = params.set('status', filter.status);
    if (filter?.source) params = params.set('source', filter.source);
    return this.http.get<Build[]>(`${this.baseUrl}/api/builds`, { params });
  }

  retrigger(id: string): Observable<RetriggerResult> {
    return this.http.post<RetriggerResult>(`${this.baseUrl}/api/builds/${id}/retrigger`, {});
  }
}
