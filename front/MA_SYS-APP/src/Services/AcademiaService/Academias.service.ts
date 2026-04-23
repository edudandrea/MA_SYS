import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';
import { HttpClient } from '@angular/common/http';
import { Modalidades } from '../ModalidadeService/Modalidades.service';

export interface Academias {
  id: number;
  nome: string;
  cidade: string;
  email: string;
  telefone: string;
  redeSocial: string;
  dataCadastro: string;
  ativo: boolean;
  totalAlunos: number;
  responsavel: string;
  totalProf: number;
  diasAtraso: number;
  slug: string;
  menuAberto?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class AcademiasService {
  private apiUrl = `${environment.apiUrl}/Academias`;

  constructor(private http: HttpClient) {}

  getAcademias() {
    return this.http.get<any[]>(this.apiUrl);
  }

  novaAcademia(acad: Partial<Academias>): Observable<Academias> {
    return this.http.post<Academias>(this.apiUrl, acad);
  }
  excluirAcademia(id: number): Observable<Academias> {
    return this.http.delete<Academias>(`${this.apiUrl}/${id}`);
  }

  atualizarAcademia(academia: Partial<Academias>): Observable<Academias> {
      return this.http.put<Academias>(`${this.apiUrl}/${academia.id}`, academia);
    }

  atualizarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, ativo);
  }
}
