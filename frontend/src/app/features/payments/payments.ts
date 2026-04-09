import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../core/services/payment';
import { CardService } from '../../core/services/card';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payments.html',
  styleUrl: './payments.css'
})
export class Payments implements OnInit {
  private paymentService = inject(PaymentService);
  private cardService = inject(CardService);

  public payments: any[] = [];
  public cards: any[] = [];
  public isLoading = true;

  // Form State
  public paymentForm = { amount: 0, recipientIdentifier: '', category: 'Transfer' };
  public isInitiating = false;
  
  // OTP State
  public pendingTransactionId = '';
  public otpCode = '';
  public isProcessing = false;
  
  public errorMessage = '';
  public successMessage = '';

  ngOnInit() {
    this.loadHistory();
    this.loadCards();
  }

  loadHistory() {
    this.paymentService.getHistory().subscribe({
      next: (history) => {
        this.payments = history || [];
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  loadCards() {
    this.cardService.getCards().subscribe({
      next: (cards) => {
        this.cards = cards || [];
      },
      error: (err) => console.error('Error loading cards', err)
    });
  }

  initiatePayment() {
    this.errorMessage = '';
    this.isInitiating = true;
    
    this.paymentService.initiateOtp(this.paymentForm).subscribe({
      next: (res) => {
        this.pendingTransactionId = res.transactionId || '00000000-0000-0000-0000-000000000000'; 
        this.isInitiating = false;
        this.successMessage = 'OTP has been sent to your registered email.';
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to initiate payment.';
        this.isInitiating = false;
      }
    });
  }

  confirmPayment() {
    this.errorMessage = '';
    
    if (this.cards.length === 0) {
      this.errorMessage = 'No cards available to process payment.';
      return;
    }

    this.isProcessing = true;

    this.paymentService.processPayment({ 
      cardId: this.cards[0].cardId || this.cards[0].id, 
      statementId: '00000000-0000-0000-0000-000000000000', 
      amount: this.paymentForm.amount,
      otpCode: this.otpCode,
      transactionId: this.pendingTransactionId,
      currency: 'INR'
    }).subscribe({
      next: (res) => {
        this.successMessage = 'Payment processed successfully!';
        this.pendingTransactionId = '';
        this.paymentForm = { amount: 0, recipientIdentifier: '', category: 'Transfer' };
        this.otpCode = '';
        this.isProcessing = false;
        this.loadHistory();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Invalid OTP or failed to process payment.';
        this.isProcessing = false;
      }
    });
  }
}
