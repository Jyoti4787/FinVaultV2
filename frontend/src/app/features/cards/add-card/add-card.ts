import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardService } from '../../../core/services/card';

@Component({
  selector: 'app-add-card',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-card.html',
  styleUrl: './add-card.css',
})
export class AddCard {
  private cardService = inject(CardService);
  private router = inject(Router);

  public newCard = {
    cardNumber: '',
    cardholderName: '',
    expiryMonth: 1,
    expiryYear: new Date().getFullYear() + 3,
    creditLimit: 5000,
    billingCycleStartDay: 1,
    issuerName: 'Credit',
  };
  
  public isSubmitting = false;
  public errorMessage = '';

  onSubmit() {
    this.isSubmitting = true;
    this.errorMessage = '';

    this.cardService.addCard(this.newCard).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/cards']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to request new card. Please verify limits.';
        this.isSubmitting = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/cards']);
  }
}
