export type BuildStatus = 'Queued' | 'Running' | 'Success' | 'Failed' | 'Canceled';
export type BuildSource = 'GitHubActions' | 'Jenkins';

export interface Build {
  id: string;
  source: BuildSource;
  repository: string;
  branch: string;
  commitSha: string;
  commitMessage: string;
  author: string;
  status: BuildStatus;
  startedAt: string;
  finishedAt: string | null;
  logsUrl: string;
}

export interface RetriggerResult {
  newBuildId: string;
  source: BuildSource;
  queuedAt: string;
}
