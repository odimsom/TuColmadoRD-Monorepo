import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DownloadInfo, DownloadService } from '../../../core/services/download.service';

@Component({
  selector: 'app-welcome',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './welcome.html',
  styleUrl: './welcome.scss'
})
export class Welcome implements OnInit {
  private readonly downloadService = inject(DownloadService);

  downloadInfo = signal<DownloadInfo | null>(null);

  ngOnInit(): void {
    this.downloadService.getLatestTestRelease().subscribe(info => {
      this.downloadInfo.set(info);
    });
  }
}
