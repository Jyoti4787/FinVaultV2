import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, AuthResponse } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/identity/auth`;

  // Generic unwrap operator
  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      // Handle the case where gateway wraps in { data: ... } or { success: true, data: ... }
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  register(payload: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/register`, payload).pipe(this.unwrap());
  }

  login(payload: any): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, payload).pipe(this.unwrap());
  }

  verifyOtp(payload: { email: string; otp: string }): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login/verify-otp`, payload).pipe(this.unwrap());
  }

  refreshToken(payload: { token: string; refreshToken: string }): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/refresh`, payload).pipe(this.unwrap());
  }

  sendMfa(payload: { email: string }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/mfa/send`, payload).pipe(this.unwrap());
  }

  verifyMfa(payload: { email: string; otp: string }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/mfa/verify`, payload).pipe(this.unwrap());
  }

  resetPassword(payload: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/reset-password`, payload).pipe(this.unwrap());
  }
}
