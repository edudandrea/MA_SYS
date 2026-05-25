import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class CadastroAlunosContext {
  slug: string = '';
}

@Injectable({
  providedIn: 'root',
})
export class CadastroAlunosService {
  private apiUrl = `${environment.apiUrl}/Alunos`;

  constructor(
    private http: HttpClient,
    private context: CadastroAlunosContext,
  ) {}

  getByCpfEmail(cpf: string, email: string) {
    return this.http.post(`${this.apiUrl}/public/${this.context.slug}/buscar`, { cpf, email });
  }

  realizarCheckIn(cpf: string, email: string, turmaId: number, dataAula: string) {
    return this.http.post(`${this.apiUrl}/public/${this.context.slug}/check-in`, { cpf, email, turmaId, dataAula });
  }

  desfazerCheckIn(cpf: string, email: string, checkInId: number) {
    return this.http.delete(`${this.apiUrl}/public/${this.context.slug}/check-in/${checkInId}`, {
      params: { cpf, email },
    });
  }

  cadastrar(slug: string, payload: any) {
    return this.http.post(`${this.apiUrl}/${slug}`, payload);
  }
}
