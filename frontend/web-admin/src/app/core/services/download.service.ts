import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

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
  private readonly releasesUrl = 'https://api.github.com/repos/odimsom/TuColmadoRD-Monorepo/releases';

  getLatestTestRelease(): Observable<DownloadInfo | null> {
    return this.http.get<GithubRelease[]>(this.releasesUrl).pipe(
      map(releases => {
        // Filtrar solo pre-releases con tag -test
        const testReleases = releases.filter(r => 
          r.prerelease && r.tag_name.includes('-test')
        );
        
        if (!testReleases.length) return null;
        
        // Tomar el más reciente (GitHub los devuelve ordenados por fecha descendente)
        const latest = testReleases[0];
        const asset = latest.assets.find(a => a.name.endsWith('.exe'));
        
        if (!asset) return null;
        
        return {
          version: latest.tag_name,
          downloadUrl: asset.browser_download_url,
          fileSize: this.formatBytes(asset.size),
          publishedAt: latest.published_at
        };
      }),
      catchError(() => of(null))
    );
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
