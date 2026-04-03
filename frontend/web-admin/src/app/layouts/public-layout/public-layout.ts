import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout {
  public themeService = inject(ThemeService);

  scrollToHero(event: Event) {
    event.preventDefault();
    const el = document.getElementById('hero-form');
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' });
    }
  }
}
