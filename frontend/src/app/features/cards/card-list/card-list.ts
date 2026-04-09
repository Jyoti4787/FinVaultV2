import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardService } from '../../../core/services/card';
import { Card } from '../../../core/interfaces/api.interfaces';

@Component({
  selector: 'app-card-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './card-list.html',
  styleUrl: './card-list.css'
})
export class CardList implements OnInit {
  private cardService = inject(CardService);
  private cdr = inject(ChangeDetectorRef);
  
  public cards: Card[] = [];
  public isLoading = true;
  public errorMessage = '';
  
  public showAddForm = false;
  public isSubmitting = false;

  // Generate dropdown year options (current year to +10 years)
  public get expiryYears(): number[] {
    const year = new Date().getFullYear();
    return Array.from({ length: 11 }, (_, i) => year + i);
  }
  public newCard = {
    cardNumber: '',
    cardholderName: '',
    cvv: '',
    cardType: 'Credit',
    expiryMonth: new Date().getMonth() + 1,
    expiryYear: new Date().getFullYear() + 3,
    creditLimit: 5000,
    billingCycleStartDay: 1,
  };

  public get detectedNetwork(): string {
    const num = this.newCard.cardNumber.replace(/\s+/g, '');
    if (num.startsWith('4')) return 'Visa';
    if (num.startsWith('5')) return 'MasterCard';
    if (num.startsWith('6')) return 'RuPay';
    if (num.startsWith('3')) return 'Amex';
    return 'Unknown';
  }

  ngOnInit() {
    this.loadCards();
  }

  loadCards() {
    this.isLoading = true;
    this.cardService.getCards().subscribe({
      next: (cards) => {
        this.cards = cards || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to load cards.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  toggleAddForm() {
    this.showAddForm = !this.showAddForm;
    if (this.showAddForm) {
      // Reset card form to blank — user enters their own real card details
      this.newCard = {
        cardNumber: '',
        cardholderName: '',
        cvv: '',
        cardType: 'Credit',
        expiryMonth: new Date().getMonth() + 1,
        expiryYear: new Date().getFullYear() + 3,
        creditLimit: 5000,
        billingCycleStartDay: 1,
      };
    }
  }

  private generateValidVisa(): string {
    const digits = [4];
    for (let i = 0; i < 14; i++) {
        digits.push(Math.floor(Math.random() * 10));
    }
    
    let sum = 0;
    for (let i = 0; i < 15; i++) {
        let d = digits[i];
        if (i % 2 === 0) {
            d *= 2;
            if (d > 9) d -= 9;
        }
        sum += d;
    }
    
    const checksum = (10 - (sum % 10)) % 10;
    digits.push(checksum);
    
    return digits.join('');
  }

  addCard() {
    this.isSubmitting = true;
    // Strip any spaces the user typed between card number groups
    const sanitizedNumber = this.newCard.cardNumber.replace(/\s+/g, '');
    const payload = {
      ...this.newCard,
      cardNumber: sanitizedNumber,
    };
    this.cardService.addCard(payload).subscribe({
      next: (card) => {
        this.cards.push(card);
        this.showAddForm = false;
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to add card.';
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  removeCard(cardId: string) {
    if (confirm('Are you sure you want to delete this card?')) {
      this.cardService.deleteCard(cardId).subscribe({
        next: () => {
          this.cards = this.cards.filter(c => c.cardId !== cardId);
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.errorMessage = 'Failed to remove card.';
          this.cdr.detectChanges();
        }
      });
    }
  }
}
