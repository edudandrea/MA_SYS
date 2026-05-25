import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { ProfessorService, Professores } from '../Services/ProfessorService/Professor.service';
import { Turma, TurmasService } from '../Services/TurmasService/Turmas.service';

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
}
