import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RewardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/payment-rewards`;

  private unwrap<T>() {
    return map((res: any) => {
      if (res && res.data !== undefined) return res.data as T;
      return res as T;
    });
  }

  getRewards(): Observable<any> {
    return this.http.get<any>(this.baseUrl).pipe(this.unwrap());
  }

  redeemReward(payload: { points: number; cardId: string }): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/redeem`, payload).pipe(this.unwrap());
  }
}
