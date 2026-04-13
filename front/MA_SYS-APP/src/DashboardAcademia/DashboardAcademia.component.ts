import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Chart } from 'chart.js';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { DashAcadService } from '../Services/DashAcademia/DashAcad.service';

@Component({
  selector: 'app-DashboardAcademia',
  templateUrl: './DashboardAcademia.component.html',
  styleUrls: ['./DashboardAcademia.component.css'],
  imports: [CommonModule, FormsModule],
})
export class DashboardAcademiaComponent implements OnInit {
  dashAcademia: any;

  constructor(private spinner: NgxSpinnerService,
              private cd: ChangeDetectorRef,
              private toastr: ToastrService,
              private dashAcad: DashAcadService) { }

  ngOnInit() {
  }

   ngAfterViewInit() {
  
    new Chart("graficoAlunos", {
      type: 'line',
      data: {
        labels: ['Jan', 'Fev', 'Mar'],
        datasets: [{
          label: 'Alunos',
          data: [10, 20, 35]
        }]
      }
    });
  
    new Chart("graficoPlanos", {
      type: 'doughnut',
      data: {
        labels: ['Mensal', 'Anual'],
        datasets: [{
          data: [60, 40]
        }]
      }
    });
  }

  loadDashboard() {
    this.spinner.show();
    this.dashAcad.getDashboard().subscribe({
      next: (res) => {
        console.log('Dashboard recebido:', res);
        this.spinner.hide();
        this.dashAcad = res;

          this.cd.markForCheck();
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Dashboard', 'Erro');
      },
    });
  }

}
