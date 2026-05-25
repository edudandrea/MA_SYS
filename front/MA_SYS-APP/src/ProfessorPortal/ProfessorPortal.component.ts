import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { ProfessorService, Professores } from '../Services/ProfessorService/Professor.service';
import { CheckInRelatorio, Turma, TurmasService } from '../Services/TurmasService/Turmas.service';

@Component({
  selector: 'app-professor-portal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ProfessorPortal.component.html',
  styleUrls: ['./ProfessorPortal.component.css'],
})
export class ProfessorPortalComponent implements OnInit {
  professores: Professores[] = [];
  turmas: Turma[] = [];
  professorId: number | null = null;
  buscaAluno = '';
  dataInicioRelatorio = '';
  dataFimRelatorio = '';
  relatorioCheckIns: CheckInRelatorio[] = [];
  relatorioCarregado = false;

  constructor(
    private professorService: ProfessorService,
    private turmasService: TurmasService,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.carregarDados();
  }

  get professorSelecionado(): Professores | null {
    return this.professores.find((professor) => professor.id === Number(this.professorId)) ?? null;
  }

  get turmasDoProfessor(): Turma[] {
    const termo = this.buscaAluno.trim().toLowerCase();

    return this.turmas
      .filter((turma) => turma.professorId === Number(this.professorId))
      .map((turma) => ({
        ...turma,
        alunos: termo
          ? turma.alunos.filter((aluno) => aluno.nome.toLowerCase().includes(termo))
          : turma.alunos,
      }));
  }

  get totalAlunos(): number {
    const ids = new Set<number>();
    this.turmasDoProfessor.forEach((turma) => turma.alunos.forEach((aluno) => ids.add(aluno.alunoId)));
    return ids.size;
  }

  get totalTurmasAtivas(): number {
    return this.turmasDoProfessor.filter((turma) => turma.ativo).length;
  }

  get totalCheckInsProximasAulas(): number {
    return this.turmasDoProfessor
      .flatMap((turma) => turma.alunos)
      .reduce((total, aluno) => total + (aluno.checkInsAulas?.length || 0), 0);
  }

  selecionarPrimeiroProfessor(): void {
    if (!this.professorId && this.professores.length) {
      this.professorId = this.professores[0].id;
    }
  }

  carregarDados(): void {
    this.spinner.show();

    forkJoin({
      professores: this.professorService.getProfessores(),
      turmas: this.turmasService.getTurmas(),
    }).subscribe({
      next: ({ professores, turmas }) => {
        this.spinner.hide();
        this.professores = (professores ?? []).filter((professor) => professor.ativo !== false);
        this.turmas = turmas ?? [];
        this.prepararPeriodoRelatorio();
        this.selecionarPrimeiroProfessor();
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Nao foi possivel carregar o portal do professor.');
      },
    });
  }

  nomesDias(turma: Turma): string {
    return turma.diasSemana?.length ? turma.diasSemana.join(', ') : 'Agenda nao informada';
  }

  desfazerCheckIn(turma: Turma, checkInId: number): void {
    this.spinner.show();
    this.turmasService.desfazerCheckIn(turma.id, checkInId).subscribe({
      next: (turmaAtualizada) => {
        this.spinner.hide();
        this.turmas = this.turmas.map((item) => item.id === turmaAtualizada.id ? turmaAtualizada : item);
        if (this.relatorioCarregado) {
          this.carregarRelatorioCheckIns();
        }
        this.toastr.success('Check-in desfeito com sucesso.');
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();
        this.toastr.error(err?.error?.message || 'Nao foi possivel desfazer o check-in.');
      },
    });
  }

  carregarRelatorioCheckIns(): void {
    this.spinner.show();
    this.turmasService.relatorioCheckIns({
      professorId: this.professorId,
      dataInicio: this.dataInicioRelatorio,
      dataFim: this.dataFimRelatorio,
    }).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.relatorioCheckIns = res ?? [];
        this.relatorioCarregado = true;
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Nao foi possivel carregar o relatorio de check-ins.');
      },
    });
  }

  formatarDataAula(data?: string): string {
    if (!data) {
      return '';
    }

    const dataIso = data.slice(0, 10);
    const partes = dataIso.split('-');
    if (partes.length === 3) {
      return `${partes[2]}/${partes[1]}`;
    }

    const date = new Date(data);
    return Number.isNaN(date.getTime())
      ? ''
      : date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
  }

  private prepararPeriodoRelatorio(): void {
    if (this.dataInicioRelatorio || this.dataFimRelatorio) {
      return;
    }

    const hoje = new Date();
    const inicio = new Date();
    inicio.setDate(hoje.getDate() - 30);
    this.dataInicioRelatorio = this.formatarInputData(inicio);
    this.dataFimRelatorio = this.formatarInputData(hoje);
  }

  private formatarInputData(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }
}
