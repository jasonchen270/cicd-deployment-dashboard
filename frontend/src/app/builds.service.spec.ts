import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { BuildsService } from './builds.service';
import { Build } from './build.model';

describe('BuildsService', () => {
  let service: BuildsService;
  let httpMock: HttpTestingController;

  const sample: Build = {
    id: 'gha-1', source: 'GitHubActions', repository: 'r', branch: 'main',
    commitSha: 'abc', commitMessage: 'm', author: 'a',
    status: 'Success', startedAt: '2026-01-01T00:00:00Z',
    finishedAt: '2026-01-01T00:05:00Z', logsUrl: 'https://x'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(BuildsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs /api/builds with no params when no filter', () => {
    service.list().subscribe(b => expect(b.length).toBe(1));
    const req = httpMock.expectOne(r => r.url === 'http://localhost:5085/api/builds');
    expect(req.request.params.keys().length).toBe(0);
    req.flush([sample]);
  });

  it('adds status and source query params when filtering', () => {
    service.list({ status: 'Failed', source: 'Jenkins' }).subscribe();
    const req = httpMock.expectOne(r => r.url === 'http://localhost:5085/api/builds');
    expect(req.request.params.get('status')).toBe('Failed');
    expect(req.request.params.get('source')).toBe('Jenkins');
    req.flush([]);
  });

  it('POSTs to the retrigger endpoint', () => {
    service.retrigger('gha-1').subscribe(r => expect(r.newBuildId).toBe('gha-99'));
    const req = httpMock.expectOne('http://localhost:5085/api/builds/gha-1/retrigger');
    expect(req.request.method).toBe('POST');
    req.flush({ newBuildId: 'gha-99', source: 'GitHubActions', queuedAt: '2026-01-01' });
  });
});
