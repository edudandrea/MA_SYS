import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../app/environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class DashAcadService {
  private apiUrl = `${environment.apiUrl}/Dashboard`;
  
  constructor(private http: HttpClient) {}

  getDashboard(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }
}
