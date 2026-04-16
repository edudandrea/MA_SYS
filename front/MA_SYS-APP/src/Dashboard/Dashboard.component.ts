import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { DashboardService } from '../Services/Dashboard/Dashboard.service';
import Chart from 'chart.js/auto';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-Dashboard',
  templateUrl: './Dashboard.component.html',
  styleUrls: ['./Dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardComponent implements OnInit {
  dashboard: any;
  isAdmin: boolean = false;

  constructor(
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private toastr: ToastrService,
    private dashService: DashboardService,
  ) {}

  ngOnInit() {
    if (typeof window !== 'undefined') {
      const role = localStorage.getItem('role');
      console.log('Role do usuário:', role);
      this.isAdmin = role === 'Admin';
    }
    this.loadDashboard();
  }

  ngAfterViewInit() {}

  loadDashboard() {
    this.spinner.show();
    this.dashService.getDashboard().subscribe({
      next: (res) => {
        console.log('Dashboard recebido:', res);
        this.spinner.hide();
        this.dashboard = res;

        setTimeout(() => {
          this.getGraficos();
        }, 100);

        this.cd.markForCheck();
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Dashboard', 'Erro');
      },
    });
  }

  getGraficos() {
    const canvasAlunos = document.getElementById('graficoAlunos') as HTMLCanvasElement;
    const canvasPlanos = document.getElementById('graficoPlanos') as HTMLCanvasElement;

    if (!canvasAlunos || !canvasPlanos) {
      console.warn('Canvas ainda não renderizado');
      return;
    }

    Chart.getChart(canvasAlunos)?.destroy();
    Chart.getChart(canvasPlanos)?.destroy();

    new Chart(canvasAlunos, {
      type: 'line',
      data: {
        labels: ['Jan', 'Fev', 'Mar', 'Abr'],
        datasets: [
          {
            label: 'Alunos',
            data: this.dashboard?.alunosPorMes || [10, 20, 30, 40],
            fill: true,
            tension: 0.4,
          },
        ],
      },
    });

    new Chart(canvasPlanos, {
      type: 'doughnut',
      data: {
        labels: this.dashboard?.planos?.map((p: any) => p.nome) || [],
        datasets: [
          {
            data: this.dashboard?.planos?.map((p: any) => p.totalAlunos) || [],
          },
        ],
      },
    });
  }
  

  onPeriodoChange(any: any) {
    console.log('Período selecionado:', any);
  }
}
