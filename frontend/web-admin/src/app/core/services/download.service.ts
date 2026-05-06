import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DownloadInfo {
  version: string;
  downloadUrl: string;
  fileSize: string;
  publishedAt: string;
}

interface GithubAsset {
  name: string;
  browser_download_url: string;
  size: number;
}

interface GithubRelease {
  tag_name: string;
  published_at: string;
  prerelease: boolean;
  assets: GithubAsset[];
}

@Injectable({
  providedIn: 'root'
})
export class DownloadService {
  private http = inject(HttpClient);
  // GitHub API — no requiere token para repos públicos
  private readonly releasesUrls = [
    'https://api.github.com/repos/odimsom/TuColmadoRD-Monorepo/releases',
    'https://api.github.com/repos/synsetsolutions/TuColmadoRD-Monorepo/releases'
  ];

  getLatestRelease(): Observable<DownloadInfo | null> {
    return this.http.get<GithubRelease[]>(this.releasesUrls[0]).pipe(
      map(releases => this.resolveStableRelease(releases)),
      catchError(() => this.http.get<GithubRelease[]>(this.releasesUrls[1]).pipe(
        map(releases => this.resolveStableRelease(releases)),
        catchError(() => of(this.getFallbackDownloadInfo()))
      ))
    );
  }

  private resolveStableRelease(releases: GithubRelease[]): DownloadInfo | null {
    // Solo releases estables: sin prerelease y sin sufijo -test
    const stable = releases.filter(r =>
      !r.prerelease && !r.tag_name.includes('-test')
    );

    if (!stable.length) return this.getFallbackDownloadInfo();

    // GitHub los devuelve ordenados por fecha descendente
    const latest = stable[0];
    const asset = latest.assets.find(a => a.name.endsWith('.exe'));

    if (!asset) return this.getFallbackDownloadInfo();

    return {
      version: latest.tag_name,
      downloadUrl: asset.browser_download_url,
      fileSize: this.formatBytes(asset.size),
      publishedAt: latest.published_at
    };
  }

  private getFallbackDownloadInfo(): DownloadInfo | null {
    if (!environment.downloadUrl) {
      return null;
    }

    return {
      version: 'latest',
      downloadUrl: environment.downloadUrl,
      fileSize: 'N/D',
      publishedAt: ''
    };
  }

  private formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = 1;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }
}
