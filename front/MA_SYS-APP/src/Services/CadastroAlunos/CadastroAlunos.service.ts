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

  cadastrar(slug: string, payload: any) {
    return this.http.post(`${this.apiUrl}/${slug}`, payload);
  }
}
