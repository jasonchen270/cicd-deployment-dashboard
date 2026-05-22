import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { BuildsListComponent } from './builds-list.component';
import { BuildsService } from './builds.service';
import { Build } from './build.model';

const make = (over: Partial<Build> = {}): Build => ({
  id: 'gha-1', source: 'GitHubActions', repository: 'octo/repo', branch: 'main',
  commitSha: 'abc1234', commitMessage: 'msg', author: 'alice',
  status: 'Success', startedAt: '2026-01-01T00:00:00Z',
  finishedAt: '2026-01-01T00:05:00Z', logsUrl: 'https://x', ...over
});

describe('BuildsListComponent', () => {
  let fixture: ComponentFixture<BuildsListComponent>;
  let component: BuildsListComponent;
  let svc: jasmine.SpyObj<BuildsService>;

  beforeEach(async () => {
    svc = jasmine.createSpyObj<BuildsService>('BuildsService', ['list', 'retrigger']);
    svc.list.and.returnValue(of([
      make({ id: 'gha-1', status: 'Success' }),
      make({ id: 'gha-2', status: 'Failed' }),
      make({ id: 'jenkins-9', source: 'Jenkins', status: 'Running', finishedAt: null })
    ]));

    await TestBed.configureTestingModule({
      imports: [BuildsListComponent],
      providers: [{ provide: BuildsService, useValue: svc }]
    }).compileComponents();

    fixture = TestBed.createComponent(BuildsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads builds on init', () => {
    expect(svc.list).toHaveBeenCalled();
    expect(component.builds().length).toBe(3);
  });

  it('computes summary counts', () => {
    const s = component.summary();
    expect(s.total).toBe(3);
    expect(s.success).toBe(1);
    expect(s.failed).toBe(1);
    expect(s.running).toBe(1);
  });

  it('renders a row per build', () => {
    const rows = fixture.nativeElement.querySelectorAll('[data-testid="build-row"]');
    expect(rows.length).toBe(3);
  });

  it('shows retrigger button only for failed/canceled builds', () => {
    const buttons = fixture.nativeElement.querySelectorAll('[data-testid="retrigger-button"]');
    expect(buttons.length).toBe(1);
  });

  it('calls service.retrigger and refreshes on success', () => {
    svc.retrigger.and.returnValue(of({
      newBuildId: 'gha-99', source: 'GitHubActions', queuedAt: '2026-01-01'
    }));
    svc.list.calls.reset();
    component.retrigger(make({ id: 'gha-2', status: 'Failed' }));
    expect(svc.retrigger).toHaveBeenCalledWith('gha-2');
    expect(svc.list).toHaveBeenCalled();
  });

  it('formats duration from start/finish', () => {
    const b = make({ startedAt: '2026-01-01T00:00:00Z', finishedAt: '2026-01-01T00:03:42Z' });
    expect(component.duration(b)).toBe('3m 42s');
  });

  it('returns a dash for unfinished builds', () => {
    expect(component.duration(make({ finishedAt: null }))).toBe('-');
  });

  it('captures error on load failure', () => {
    svc.list.and.returnValue(throwError(() => ({ message: 'boom' })));
    component.reload();
    expect(component.error()).toContain('boom');
  });
});
