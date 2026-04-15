import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RewardService } from '../../core/services/reward';
import { CardService } from '../../core/services/card';
import { Card } from '../../core/interfaces/api.interfaces';
import { timeout, catchError, of } from 'rxjs';

@Component({
  selector: 'app-rewards',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rewards.html',
  styleUrl: './rewards.css'
})
export class Rewards implements OnInit {
  private rewardService = inject(RewardService);
  private cardService = inject(CardService);
  private cdr = inject(ChangeDetectorRef);

  public points = 0;
  public cashbackValue = 0;
  public history: any[] = [];
  public isLoading = true;
  public errorMessage = '';
  public successMessage = '';
  public isRedeeming = false;

  // Credit Cards
  public cards: Card[] = [];
  public selectedCardId: string = '';

  // Redemption form
  public redeemPoints = 100;

  get redeemCashback(): number {
    return Math.round(this.redeemPoints * 0.10 * 100) / 100;
  }

  ngOnInit() {
    this.loadRewards();
    this.loadCards();
  }

  loadCards() {
    this.cardService.getCards().subscribe({
      next: (res: any) => {
        // CardService.getCards() already calls unwrap, so res is the data array
        this.cards = Array.isArray(res) ? res : (res?.data || []);
        if (this.cards.length > 0) {
          this.selectedCardId = this.cards[0].cardId;
        }
        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Failed to load cards:', err)
    });
  }

  loadRewards() {
    this.isLoading = true;
    this.rewardService.getRewards().pipe(
      timeout(10000),
      catchError(err => {
        console.error('Reward loading failed:', err);
        return of({ Points: 0, CashbackValue: 0, History: [] });
      })
    ).subscribe({
      next: (res: any) => {
        this.points = res.Points ?? res.points ?? 0;
        this.cashbackValue = res.CashbackValue ?? res.cashbackValue ?? 0;
        const raw = res.History ?? res.history ?? [];
        this.history = Array.isArray(raw) ? raw : [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load rewards.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  redeem() {
    this.errorMessage = '';
    this.successMessage = '';

    if (this.redeemPoints < 100) {
      this.errorMessage = 'Minimum 100 points required to redeem.';
      return;
    }
    if (this.redeemPoints > this.points) {
      this.errorMessage = `You only have ${this.points} points available.`;
      return;
    }
    if (!this.selectedCardId) {
      this.errorMessage = 'Please select a card to apply the credit to.';
      return;
    }

    this.isRedeeming = true;
    this.rewardService.redeemReward({ 
      points: this.redeemPoints, 
      cardId: this.selectedCardId 
    }).subscribe({
      next: (res: any) => {
        this.successMessage = res.Message ?? res.message ?? `Redeemed ${this.redeemPoints} points for ₹${this.redeemCashback} cashback!`;
        this.isRedeeming = false;
        this.loadRewards();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || err.error?.data?.message || 'Redemption failed.';
        this.isRedeeming = false;
        this.cdr.detectChanges();
      }
    });
  }
}
