import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, Payment, ProcessPaymentRequest } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/payments`;

  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  processPayment(payload: ProcessPaymentRequest): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/process`, payload).pipe(this.unwrap());
  }

  initiateOtp(payload?: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/initiate-otp`, payload || {}).pipe(this.unwrap());
  }

  getHistory(): Observable<Payment[]> {
    return this.http.get<ApiResponse<Payment[]>>(`${this.baseUrl}/history`).pipe(this.unwrap());
  }
}
