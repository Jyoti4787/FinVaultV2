import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, SupportTicket, CreateTicketRequest } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class SupportService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/support`;

  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  getTickets(): Observable<SupportTicket[]> {
    return this.http.get<ApiResponse<SupportTicket[]>>(`${this.baseUrl}/tickets`).pipe(this.unwrap());
  }

  createTicket(payload: CreateTicketRequest): Observable<SupportTicket> {
    return this.http.post<ApiResponse<SupportTicket>>(`${this.baseUrl}/tickets`, payload).pipe(this.unwrap());
  }

  getAdminTickets(): Observable<SupportTicket[]> {
    return this.http.get<ApiResponse<SupportTicket[]>>(`${this.baseUrl}/admin/tickets`).pipe(this.unwrap());
  }

  resolveTicket(id: string, comment: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/admin/tickets/${id}/resolve`, { comment });
  }
}
