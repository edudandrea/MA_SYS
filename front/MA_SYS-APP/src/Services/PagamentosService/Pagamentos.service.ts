import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { Observable } from 'rxjs/internal/Observable';
import { PixResponse } from '../../Model/pix-response.model';


export interface Pagamentos {
  id: number;
  nome: string;
  ativo: boolean;
  taxa: number;
  parcelas: number;
  dias: number;
}

@Injectable({
  providedIn: 'root',
})
export class PagamentosService {
  private apiUrl = `${environment.apiUrl}/Pagamentos`;
  private apiUrlFormaPagamento = `${environment.apiUrl}/FormaPagamento`;
  private apiUrlPix = `${environment.apiUrl}/Pix`;

  constructor(private http: HttpClient) {}

  getFormaPagamentos(): Observable<Pagamentos[]> {
    return this.http.get<Pagamentos[]>(this.apiUrlFormaPagamento);
  }

  novaFormaPgto(formaPg: Partial<Pagamentos>): Observable<Pagamentos> {
    return this.http.post<Pagamentos>(this.apiUrlFormaPagamento, formaPg);
  }

   atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }

  gerarPix(valor: number): Observable<PixResponse> {
    return this.http.post<PixResponse>(`${this.apiUrlPix}/pix`, { valor });
  }
}
