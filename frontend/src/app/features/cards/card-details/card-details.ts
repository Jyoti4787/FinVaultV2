import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CardService } from '../../../core/services/card';
import { Card } from '../../../core/interfaces/api.interfaces';

@Component({
  selector: 'app-card-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './card-details.html',
  styleUrl: './card-details.css',
})
export class CardDetails implements OnInit {
  private cardService = inject(CardService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  public card: Card | null = null;
  public utilization: any = null;
  public isLoading = true;
  public isUpdating = false;
  public isDeleting = false;
  public errorMessage = '';
  public successMessage = '';
  public newLimit = 0;
  public showLimitForm = false;
  private cardId = '';

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.cardId = params['id'];
      this.loadCard();
      this.loadUtilization();
    });
  }

  loadCard() {
    this.isLoading = true;
    this.cardService.getCard(this.cardId).subscribe({
      next: (card) => {
        this.card = card;
        this.newLimit = card.creditLimit;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load card details.';
        this.isLoading = false;
      }
    });
  }

  loadUtilization() {
    this.cardService.getUtilization(this.cardId).subscribe({
      next: (data) => this.utilization = data,
      error: () => {} // Silently fail utilization
    });
  }

  setDefault() {
    this.isUpdating = true;
    this.cardService.setDefaultCard(this.cardId).subscribe({
      next: () => {
        if (this.card) this.card.isDefault = true;
        this.successMessage = 'Card set as default payment method.';
        this.isUpdating = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to set default card.';
        this.isUpdating = false;
      }
    });
  }

  updateLimit() {
    this.isUpdating = true;
    this.cardService.updateLimit(this.cardId, { newLimit: this.newLimit }).subscribe({
      next: () => {
        if (this.card) this.card.creditLimit = this.newLimit;
        this.successMessage = 'Credit limit updated successfully.';
        this.showLimitForm = false;
        this.isUpdating = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to update credit limit.';
        this.isUpdating = false;
      }
    });
  }

  deleteCard() {
    if (!confirm('Are you sure you want to remove this card? This action cannot be undone.')) return;
    this.isDeleting = true;
    this.cardService.deleteCard(this.cardId).subscribe({
      next: () => {
        this.router.navigate(['/cards']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to remove card.';
        this.isDeleting = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/cards']);
  }

  get utilizationPercent(): number {
    if (!this.card) return 0;
    return Math.round((this.card.outstandingBalance / this.card.creditLimit) * 100);
  }

  get utilizationColor(): string {
    const p = this.utilizationPercent;
    if (p < 30) return '#2e8a5c';
    if (p < 70) return '#e4b66b';
    return '#8a3b2e';
  }
}
