import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, Reward, RedeemRewardRequest, RewardSummary } from '../interfaces/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class RewardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/rewards`;

  private unwrap<T>() {
    return map((res: ApiResponse<T> | any) => {
      if (res && res.data !== undefined) {
        return res.data as T;
      }
      return res as T;
    });
  }

  getRewards(): Observable<Reward[] | RewardSummary> {
    return this.http.get<ApiResponse<Reward[] | RewardSummary>>(this.baseUrl).pipe(this.unwrap());
  }

  redeemReward(payload: RedeemRewardRequest | { amount: number }): Observable<any> {
    // Convert amount to points if needed
    const requestPayload = 'amount' in payload && !('points' in payload)
      ? { points: payload.amount, reason: 'Reward redemption' }
      : payload;
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/redeem`, requestPayload).pipe(this.unwrap());
  }
}
