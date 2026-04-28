import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { DashboardService } from '../Services/Dashboard/Dashboard.service';
import Chart from 'chart.js/auto';
import { ToastrService } from 'ngx-toastr';
import { AfterViewInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'app-Dashboard',
  templateUrl: './Dashboard.component.html',
  styleUrls: ['./Dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  dashboard: any;
  role = '';
  private themeObserver?: MutationObserver;

  constructor(
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private toastr: ToastrService,
    private dashService: DashboardService,
  ) {}

  ngOnInit() {
    if (typeof window !== 'undefined') {
      const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
      this.role = usuario.role || '';
    }

    this.loadDashboard();
  }

  ngAfterViewInit() {
    if (typeof document === 'undefined') {
      return;
    }

    this.themeObserver = new MutationObserver((mutations) => {
      const themeChanged = mutations.some(
        (mutation) => mutation.type === 'attributes' && mutation.attributeName === 'data-theme',
      );

      if (themeChanged && this.dashboard) {
        this.renderCharts();
      }
    });

    this.themeObserver.observe(document.body, {
      attributes: true,
      attributeFilter: ['data-theme'],
    });
  }

  ngOnDestroy() {
    this.themeObserver?.disconnect();
  }

  loadDashboard() {
    this.spinner.show();
    this.dashService.getDashboard().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.dashboard = res;

        setTimeout(() => {
          this.renderCharts();
        }, 100);

        this.cd.markForCheck();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar o dashboard', 'Erro');
      },
    });
  }

  renderCharts() {
    const canvasAlunos = document.getElementById('graficoAlunos') as HTMLCanvasElement | null;
    const canvasPlanos = document.getElementById('graficoPlanos') as HTMLCanvasElement | null;

    if (canvasAlunos) {
      Chart.getChart(canvasAlunos)?.destroy();
    }
    if (canvasPlanos) {
      Chart.getChart(canvasPlanos)?.destroy();
    }

    const textColor = this.getCssVar('--text-secondary');
    const legendColor = this.isDarkThemeActive()
      ? this.getCssVar('--text-primary')
      : '#1f2937';
    const gridColor = 'rgba(148, 163, 184, 0.14)';
    const accentPrimary = this.getCssVar('--accent-primary');
    const accentStrong = this.getCssVar('--accent-primary-strong');
    const accentSecondary = this.getCssVar('--accent-secondary');
    const planosData = this.dashboard?.planos?.map((p: any) => p.totalAlunos) || [];
    const isDarkTheme = this.isDarkThemeActive();
    const planoPalette = isDarkTheme
      ? [
          '#60a5fa', // blue
          '#fbbf24', // amber
          '#4ade80', // green
          '#f87171', // red
          '#c084fc', // purple
          '#2dd4bf', // teal
          '#f472b6', // pink
          '#fde047', // yellow
          accentPrimary,
          accentSecondary,
        ]
      : [
          '#1d4ed8', // blue
          '#b45309', // amber
          '#166534', // green
          '#b91c1c', // red
          '#6d28d9', // purple
          '#0f766e', // teal
          '#be185d', // pink
          '#a16207', // yellow
          accentPrimary,
          accentSecondary,
        ];

    if (canvasAlunos) {
      new Chart(canvasAlunos, {
        type: 'line',
        data: {
          labels: ['Jan', 'Fev', 'Mar', 'Abr'],
          datasets: [
            {
              label: 'Alunos',
              data: this.dashboard?.alunosPorMes || [10, 20, 30, 40],
              fill: true,
              tension: 0.35,
              borderColor: accentPrimary,
              backgroundColor: 'rgba(37, 99, 235, 0.16)',
              pointBackgroundColor: accentStrong,
              pointBorderColor: '#ffffff',
              pointRadius: 4,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              labels: {
                color: textColor,
              },
            },
          },
          scales: {
            x: {
              ticks: { color: textColor },
              grid: { color: gridColor },
            },
            y: {
              ticks: { color: textColor },
              grid: { color: gridColor },
              beginAtZero: true,
            },
          },
        },
      });
    }

    if (canvasPlanos) {
      new Chart(canvasPlanos, {
        type: 'doughnut',
        data: {
          labels: this.dashboard?.planos?.map((p: any) => p.nome) || [],
          datasets: [
            {
              data: planosData,
              backgroundColor: planosData.map((_: number, index: number) => planoPalette[index % planoPalette.length]),
              borderColor: this.getCssVar('--bg-panel'),
              borderWidth: 2,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              labels: {
                color: legendColor,
                padding: 18,
              },
            },
          },
        },
      });
    }
  }

  onPeriodoChange(_: Event) {}

  get isAdmin(): boolean {
    return this.role === 'Admin';
  }

  get isSuperAdmin(): boolean {
    return this.role === 'SuperAdmin';
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  private getCssVar(name: string) {
    return getComputedStyle(document.body).getPropertyValue(name).trim() || '#94a3b8';
  }

  private isDarkThemeActive() {
    if (typeof document === 'undefined') {
      return true;
    }

    const theme = this.getCurrentTheme();
    return ['system', 'green-gold', 'red-black'].includes(theme);
  }

  private getCurrentTheme() {
    if (typeof document === 'undefined') {
      return 'system';
    }

    return document.body.getAttribute('data-theme') || 'system';
  }
}
