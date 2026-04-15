import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, Card, CardUtilization, AddCardRequest } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/cards`;
  private identityUrl = `${environment.apiUrl}/identity/auth`;

  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  getCards(): Observable<Card[]> {
    return this.http.get<ApiResponse<Card[]>>(this.baseUrl).pipe(this.unwrap());
  }

  getCard(cardId: string): Observable<Card> {
    return this.http.get<ApiResponse<Card>>(`${this.baseUrl}/${cardId}`).pipe(this.unwrap());
  }

  getUtilization(cardId: string): Observable<CardUtilization> {
    return this.http.get<ApiResponse<CardUtilization>>(`${this.baseUrl}/${cardId}/utilization`).pipe(this.unwrap());
  }

  addCard(payload: AddCardRequest): Observable<Card> {
    return this.http.post<ApiResponse<Card>>(this.baseUrl, payload).pipe(this.unwrap());
  }

  deleteCard(cardId: string): Observable<any> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/${cardId}`).pipe(this.unwrap());
  }

  setDefaultCard(cardId: string): Observable<any> {
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/${cardId}/default`, {}).pipe(this.unwrap());
  }

  verifyCard(cardId: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${cardId}/verify`, {}).pipe(this.unwrap());
  }

  updateLimit(cardId: string, newLimit: number | { newLimit: number }): Observable<any> {
    const payload = typeof newLimit === 'number' ? { newLimit } : newLimit;
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/${cardId}/limit`, payload).pipe(this.unwrap());
  }

  /** Send OTP for card reveal via identity-service MFA endpoint */
  sendRevealOtp(email: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.identityUrl}/mfa/send`, {
      email,
      purpose: 'CardReveal',
      correlationId: '00000000-0000-0000-0000-000000000000'
    }).pipe(this.unwrap());
  }

  /** Reveal card details with OTP verification (POST) */
  revealCard(cardId: string, otpCode: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${cardId}/reveal`, {
      otpCode
    }).pipe(this.unwrap());
  }

  getAdminCards(): Observable<Card[]> {
    return this.http.get<ApiResponse<Card[]>>(`${this.baseUrl}/admin/all`).pipe(this.unwrap());
  }

  approveAdminCard(cardId: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/admin/${cardId}/approve`, {}).pipe(this.unwrap());
  }
}
