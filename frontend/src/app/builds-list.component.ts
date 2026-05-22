import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BuildsService } from './builds.service';
import { Build, BuildSource, BuildStatus } from './build.model';

@Component({
  selector: 'app-builds-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container">
      <header class="header">
        <h1>CI/CD Deployment Dashboard</h1>
        <p class="subtitle">Unified view across GitHub Actions and Jenkins</p>
      </header>

      <div class="controls">
        <label>
          Status
          <select [(ngModel)]="statusFilter" (ngModelChange)="reload()">
            <option [ngValue]="undefined">All</option>
            <option *ngFor="let s of statuses" [ngValue]="s">{{ s }}</option>
          </select>
        </label>
        <label>
          Source
          <select [(ngModel)]="sourceFilter" (ngModelChange)="reload()">
            <option [ngValue]="undefined">All</option>
            <option *ngFor="let s of sources" [ngValue]="s">{{ s }}</option>
          </select>
        </label>
        <button class="refresh" (click)="reload()" [disabled]="loading()">
          {{ loading() ? 'Loading...' : 'Refresh' }}
        </button>
      </div>

      <div class="stats">
        <span class="stat"><b>{{ summary().total }}</b> total</span>
        <span class="stat success">✓ {{ summary().success }}</span>
        <span class="stat failed">✗ {{ summary().failed }}</span>
        <span class="stat running">● {{ summary().running }}</span>
      </div>

      <div class="error" *ngIf="error()">{{ error() }}</div>

      <table class="builds" *ngIf="!error()">
        <thead>
          <tr>
            <th>Status</th>
            <th>Source</th>
            <th>Repository / Branch</th>
            <th>Commit</th>
            <th>Author</th>
            <th>Started</th>
            <th>Duration</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let b of builds(); trackBy: trackById" data-testid="build-row">
            <td><span class="badge {{ b.status.toLowerCase() }}">{{ b.status }}</span></td>
            <td>{{ b.source === 'GitHubActions' ? 'GitHub' : 'Jenkins' }}</td>
            <td>
              <div class="repo">{{ b.repository }}</div>
              <div class="branch">{{ b.branch }}</div>
            </td>
            <td>
              <code>{{ b.commitSha }}</code>
              <div class="msg">{{ b.commitMessage }}</div>
            </td>
            <td>{{ b.author }}</td>
            <td>{{ formatTime(b.startedAt) }}</td>
            <td>{{ duration(b) }}</td>
            <td>
              <button
                class="retrigger"
                *ngIf="b.status === 'Failed' || b.status === 'Canceled'"
                (click)="retrigger(b)"
                [disabled]="retriggering() === b.id"
                data-testid="retrigger-button"
              >
                {{ retriggering() === b.id ? '...' : 'Retrigger' }}
              </button>
            </td>
          </tr>
          <tr *ngIf="!loading() && builds().length === 0">
            <td colspan="8" class="empty">No builds match the current filters.</td>
          </tr>
        </tbody>
      </table>

      <div class="toast" *ngIf="toast()">{{ toast() }}</div>
    </section>
  `,
  styles: [`
    .container { max-width: 1200px; margin: 0 auto; padding: 32px 24px; }
    .header h1 { margin: 0 0 4px; font-size: 28px; color: var(--accent); }
    .subtitle { margin: 0 0 24px; color: var(--muted); }
    .controls { display: flex; gap: 16px; align-items: end; margin-bottom: 16px; }
    .controls label { display: flex; flex-direction: column; gap: 4px; color: var(--muted); font-size: 12px; }
    .controls select {
      background: var(--panel); color: var(--text); border: 1px solid var(--border);
      padding: 6px 10px; border-radius: 6px; min-width: 140px;
    }
    .refresh {
      background: var(--accent); color: #0f172a; border: 0; padding: 8px 16px;
      border-radius: 6px; font-weight: 600;
    }
    .refresh:disabled { opacity: 0.6; cursor: not-allowed; }
    .stats { display: flex; gap: 16px; margin-bottom: 16px; }
    .stat { background: var(--panel); padding: 6px 12px; border-radius: 6px; font-size: 13px; }
    .stat.success { color: var(--green); }
    .stat.failed { color: var(--red); }
    .stat.running { color: var(--blue); }
    .error { background: #7f1d1d; padding: 12px 16px; border-radius: 6px; margin-bottom: 16px; }
    .builds { width: 100%; border-collapse: collapse; background: var(--panel); border-radius: 8px; overflow: hidden; }
    .builds th { text-align: left; padding: 12px; background: #0b1220; color: var(--muted); font-size: 12px; text-transform: uppercase; }
    .builds td { padding: 12px; border-top: 1px solid var(--border); font-size: 14px; vertical-align: top; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; }
    .badge.success { background: rgba(34, 197, 94, 0.15); color: var(--green); }
    .badge.failed { background: rgba(239, 68, 68, 0.15); color: var(--red); }
    .badge.running { background: rgba(59, 130, 246, 0.15); color: var(--blue); }
    .badge.queued { background: rgba(245, 158, 11, 0.15); color: var(--amber); }
    .badge.canceled { background: rgba(100, 116, 139, 0.2); color: var(--gray); }
    .repo { font-weight: 500; }
    .branch { font-size: 12px; color: var(--muted); }
    code { background: #0b1220; padding: 2px 6px; border-radius: 4px; font-size: 12px; }
    .msg { font-size: 12px; color: var(--muted); margin-top: 2px; max-width: 280px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .retrigger {
      background: transparent; color: var(--accent); border: 1px solid var(--accent);
      padding: 4px 12px; border-radius: 4px; font-size: 12px;
    }
    .retrigger:disabled { opacity: 0.5; }
    .empty { text-align: center; padding: 32px; color: var(--muted); }
    .toast {
      position: fixed; bottom: 24px; right: 24px;
      background: var(--green); color: #0f172a; padding: 12px 16px;
      border-radius: 6px; font-weight: 500;
    }
  `]
})
export class BuildsListComponent implements OnInit {
  private readonly service = inject(BuildsService);

  readonly statuses: BuildStatus[] = ['Queued', 'Running', 'Success', 'Failed', 'Canceled'];
  readonly sources: BuildSource[] = ['GitHubActions', 'Jenkins'];

  statusFilter: BuildStatus | undefined;
  sourceFilter: BuildSource | undefined;

  builds = signal<Build[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  retriggering = signal<string | null>(null);
  toast = signal<string | null>(null);

  summary = () => {
    const list = this.builds();
    return {
      total: list.length,
      success: list.filter(b => b.status === 'Success').length,
      failed: list.filter(b => b.status === 'Failed').length,
      running: list.filter(b => b.status === 'Running').length
    };
  };

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.list({ status: this.statusFilter, source: this.sourceFilter }).subscribe({
      next: builds => {
        this.builds.set(builds);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(`Failed to load builds: ${err.message ?? err}`);
        this.loading.set(false);
      }
    });
  }

  retrigger(b: Build): void {
    this.retriggering.set(b.id);
    this.service.retrigger(b.id).subscribe({
      next: result => {
        this.retriggering.set(null);
        this.toast.set(`Retriggered ${b.id} → ${result.newBuildId}`);
        setTimeout(() => this.toast.set(null), 3000);
        this.reload();
      },
      error: err => {
        this.retriggering.set(null);
        this.error.set(`Retrigger failed: ${err.message ?? err}`);
      }
    });
  }

  trackById(_: number, b: Build): string {
    return b.id;
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleString();
  }

  duration(b: Build): string {
    if (!b.finishedAt) return '-';
    const ms = new Date(b.finishedAt).getTime() - new Date(b.startedAt).getTime();
    const m = Math.floor(ms / 60000);
    const s = Math.floor((ms % 60000) / 1000);
    return `${m}m ${s}s`;
  }
}
