import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, User } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root',
})
export class IdentityUser {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/identity/users`;

  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  getProfile(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/profile`).pipe(this.unwrap());
  }

  uploadProfilePicture(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/profile/picture`, formData).pipe(this.unwrap());
  }

  getProfilePictureUrl(userId: string): string {
    return `${this.baseUrl}/profile/picture/${userId}`;
  }
}
