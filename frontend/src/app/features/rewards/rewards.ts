import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RewardService } from '../../core/services/reward';
import { timeout, catchError, of } from 'rxjs';

@Component({
  selector: 'app-rewards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rewards.html',
  styleUrl: './rewards.css'
})
export class Rewards implements OnInit {
  private rewardService = inject(RewardService);
  private cdr = inject(ChangeDetectorRef);
  
  public points = 0;
  public history: any[] = [];
  public isLoading = true;
  public errorMessage = '';
  public successMessage = '';
  public isRedeeming = false;

  ngOnInit() {
    this.rewardService.getRewards().pipe(
      timeout(10000),
      catchError(err => {
        console.error('Reward loading failed:', err);
        return of({ points: 0, history: [] });
      })
    ).subscribe({
      next: (res) => {
        if (Array.isArray(res)) {
          this.history = res;
          this.points = 0;
        } else {
          this.points = res.points || 0;
          this.history = res.history || [];
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to load rewards data.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  redeem() {
    if (this.points < 1000) {
      this.errorMessage = 'Minimum 1000 points required to redeem.';
      return;
    }

    this.isRedeeming = true;
    this.errorMessage = '';
    
    this.rewardService.redeemReward({ amount: 1000 }).subscribe({
      next: () => {
        this.successMessage = 'Successfully redeemed 1000 points!';
        this.points -= 1000;
        this.isRedeeming = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Redemption failed.';
        this.isRedeeming = false;
      }
    });
  }
}
