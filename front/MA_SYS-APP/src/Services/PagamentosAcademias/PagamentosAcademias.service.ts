import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';

export interface PagamentoAcademia {
  id: number;
  academiaId: number;
  nomeAcademia: string;
  valor: number;
  dataCriacao: string;
  dataVencimento: string;
  dataPagamento?: string;
  status: string;
  descricao?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PagamentosAcademiasService {
  private apiUrl = `${environment.apiUrl}/PagamentosAcademias`;

  constructor(private http: HttpClient) {}

  listar(): Observable<PagamentoAcademia[]> {
    return this.http.get<PagamentoAcademia[]>(this.apiUrl);
  }

  listarPorAcademia(academiaId: number): Observable<PagamentoAcademia[]> {
    return this.http.get<PagamentoAcademia[]>(`${this.apiUrl}?academiaId=${academiaId}`);
  }

  criarCobranca(payload: {
    academiaId: number;
    valor: number;
    dataVencimento: string;
    descricao?: string;
  }): Observable<any> {
    return this.http.post(this.apiUrl, payload);
  }

  baixar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/baixar`, {});
  }
}
