import { Component, inject, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CardService } from '../../../core/services/card';
import { Card } from '../../../core/interfaces/api.interfaces';

@Component({
  selector: 'app-card-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './card-list.html',
  styleUrl: './card-list.css'
})
export class CardList implements OnInit {
  private cardService = inject(CardService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  
  public cards: Card[] = [];
  public isLoading = true;
  public errorMessage = '';
  
  public showAddForm = false;
  public isSubmitting = false;

  public activeDropdownId: string | null = null;
  public revealedCardId: string | null = null;

  // ── OTP Modal State ──────────────────────────────────────────
  public showOtpModal = false;
  public otpCode = '';
  public otpCardId: string | null = null;
  public otpLoading = false;
  public otpSending = false;
  public otpError = '';
  public otpSent = false;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    this.activeDropdownId = null;
  }

  toggleDropdown(cardId: string, event?: Event) {
    if (event) event.stopPropagation();
    this.activeDropdownId = this.activeDropdownId === cardId ? null : cardId;
  }

  navigateToDetails(card: Card, event?: Event) {
    if (event) event.stopPropagation();
    this.router.navigate(['/cards', card.cardId], { state: { card } });
  }

  /** Step 1: User clicks reveal → send OTP email and show modal */
  toggleReveal(cardId: string, event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    
    // If already revealed, just hide
    if (this.revealedCardId === cardId) {
      this.revealedCardId = null;
      return;
    }

    // Open OTP modal and send OTP
    this.otpCardId = cardId;
    this.otpCode = '';
    this.otpError = '';
    this.otpSent = false;
    this.showOtpModal = true;
    this.cdr.detectChanges();

    this.sendRevealOtp();
  }

  /** Send (or resend) the OTP email for card reveal */
  sendRevealOtp() {
    const email = localStorage.getItem('user_email');
    if (!email) {
      this.otpError = 'Could not find your email. Please log in again.';
      this.cdr.detectChanges();
      return;
    }

    this.otpSending = true;
    this.otpError = '';
    this.cdr.detectChanges();

    this.cardService.sendRevealOtp(email).subscribe({
      next: () => {
        this.otpSending = false;
        this.otpSent = true;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Error sending reveal OTP:', err);
        this.otpSending = false;
        this.otpError = err.error?.message || 'Failed to send OTP. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  /** Step 2: User submits OTP → verify and reveal card */
  submitRevealOtp() {
    if (!this.otpCode || !this.otpCardId) return;

    this.otpLoading = true;
    this.otpError = '';
    this.cdr.detectChanges();

    this.cardService.revealCard(this.otpCardId, this.otpCode).subscribe({
      next: (revealedData: any) => {
        const fullNumber = revealedData.cardNumber || revealedData.CardNumber || 
                          revealedData.fullNumber || revealedData.FullNumber;
        const cvv = revealedData.cvv || revealedData.CVV || revealedData.Cvv;
        
        // Update the card in the list with revealed details
        const cardIndex = this.cards.findIndex(c => c.cardId === this.otpCardId);
        if (cardIndex !== -1) {
          this.cards[cardIndex] = { 
            ...this.cards[cardIndex], 
            fullNumber: fullNumber,
            cvv: cvv
          };
        }
        
        this.revealedCardId = this.otpCardId;
        this.otpLoading = false;
        this.showOtpModal = false;
        this.otpCode = '';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Error revealing card:', err);
        this.otpLoading = false;
        this.otpError = err.error?.message || 'Invalid OTP. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  /** Close the OTP modal */
  closeOtpModal(event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.showOtpModal = false;
    this.otpCardId = null;
    this.otpCode = '';
    this.otpError = '';
    this.cdr.detectChanges();
  }

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
      next: (cards: Card[]) => {
        this.cards = cards || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
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

  addCard() {
    this.isSubmitting = true;
    // Strip any spaces the user typed between card number groups
    const sanitizedNumber = this.newCard.cardNumber.replace(/\s+/g, '');
    const payload = {
      ...this.newCard,
      cardNumber: sanitizedNumber,
    };
    this.cardService.addCard(payload).subscribe({
      next: (response: any) => {
        console.log('Add card response:', response);
        
        // Map the response to Card interface
        const newCard: Card = {
          cardId: response.cardId || response.CardId,
          maskedNumber: response.maskedNumber || response.MaskedNumber,
          cardholderName: this.newCard.cardholderName,
          issuerName: response.issuerName || response.IssuerName || 'Visa',
          network: response.issuerName || response.IssuerName || 'Visa',
          expiryMonth: this.newCard.expiryMonth,
          expiryYear: this.newCard.expiryYear,
          creditLimit: this.newCard.creditLimit,
          outstandingBalance: 0,
          utilizationPercent: 0,
          billingCycleStartDay: this.newCard.billingCycleStartDay,
          isDefault: false,
          isVerified: false
        };
        
        this.cards.push(newCard);
        this.showAddForm = false;
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Add card error:', err);
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
        error: (err: any) => {
          this.errorMessage = 'Failed to remove card.';
          this.cdr.detectChanges();
        }
      });
    }
  }
}
