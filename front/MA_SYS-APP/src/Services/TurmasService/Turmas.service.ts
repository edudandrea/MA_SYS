import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface TurmaAluno {
  alunoId: number;
  nome: string;
  checkInProximaAula?: boolean;
  dataCheckInProximaAula?: string;
  diaSemanaCheckIn?: string;
  checkInsAulas?: TurmaAlunoCheckIn[];
}

export interface TurmaAlunoCheckIn {
  checkInId: number;
  dataAula: string;
  diaSemana: string;
}

export interface CheckInRelatorio {
  checkInId: number;
  alunoId: number;
  alunoNome: string;
  turmaId: number;
  turmaNome: string;
  professorId?: number | null;
  professorNome?: string | null;
  dataAula: string;
  diaSemana: string;
  origem?: string;
}

export interface Turma {
  id: number;
  nome: string;
  descricao?: string;
  professorId?: number | null;
  professorNome?: string | null;
  diasSemana: string[];
  ativo: boolean;
  alunos: TurmaAluno[];
}

@Injectable({
  providedIn: 'root',
})
export class TurmasService {
  private apiUrl = `${environment.apiUrl}/Turmas`;

  constructor(private http: HttpClient) {}

  getTurmas(): Observable<Turma[]> {
    return this.http.get<Turma[]>(this.apiUrl);
  }

  salvarTurma(payload: Partial<Turma>): Observable<Turma> {
    return this.http.post<Turma>(this.apiUrl, payload);
  }

  atualizarTurma(id: number, payload: Partial<Turma>): Observable<Turma> {
    return this.http.put<Turma>(`${this.apiUrl}/${id}`, payload);
  }

  adicionarAluno(turmaId: number, alunoId: number): Observable<Turma> {
    return this.http.post<Turma>(`${this.apiUrl}/${turmaId}/alunos/${alunoId}`, {});
  }

  removerAluno(turmaId: number, alunoId: number): Observable<Turma> {
    return this.http.delete<Turma>(`${this.apiUrl}/${turmaId}/alunos/${alunoId}`);
  }

  desfazerCheckIn(turmaId: number, checkInId: number): Observable<Turma> {
    return this.http.delete<Turma>(`${this.apiUrl}/${turmaId}/check-ins/${checkInId}`);
  }

  relatorioCheckIns(params: { professorId?: number | null; dataInicio?: string; dataFim?: string }): Observable<CheckInRelatorio[]> {
    const query: any = {};
    if (params.professorId) query.professorId = params.professorId;
    if (params.dataInicio) query.dataInicio = params.dataInicio;
    if (params.dataFim) query.dataFim = params.dataFim;
    return this.http.get<CheckInRelatorio[]>(`${this.apiUrl}/check-ins/relatorio`, { params: query });
  }

  excluirTurma(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
