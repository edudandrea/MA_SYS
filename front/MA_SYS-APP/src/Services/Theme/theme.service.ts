import { Injectable } from '@angular/core';

export type ThemeOption = {
  value: string;
  label: string;
};

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly storageKey = 'ma_sys_theme';
  private readonly defaultTheme = 'system';

  readonly themes: ThemeOption[] = [
    { value: 'system', label: 'UNiFlow Dark' },
    { value: 'blue-light', label: 'Dojo Claro' },
    { value: 'yellow-white', label: 'Lima + Branco' },
    { value: 'purple-white', label: 'Grafite + Verde' },
    { value: 'green-gold', label: 'Verde Profundo' },
    { value: 'red-black', label: 'Preto Tecnico' },
    { value: 'windows', label: 'Operacional Claro' },
    { value: 'sage-sand', label: 'Sage Executivo' },
    { value: 'navy-mint', label: 'Marinho + Menta' },
    { value: 'platinum-teal', label: 'Platina + Teal' },
  ];

  initializeTheme() {
    if (typeof document === 'undefined') {
      return;
    }

    const theme = localStorage.getItem(this.storageKey) || this.defaultTheme;
    this.applyTheme(theme);
  }

  applyTheme(theme: string) {
    if (typeof document === 'undefined') {
      return;
    }

    document.body.setAttribute('data-theme', theme);
    localStorage.setItem(this.storageKey, theme);
  }

  getCurrentTheme() {
    if (typeof document === 'undefined') {
      return this.defaultTheme;
    }

    return document.body.getAttribute('data-theme') || localStorage.getItem(this.storageKey) || this.defaultTheme;
  }
}
