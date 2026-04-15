import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../core/services/payment';
import { CardService } from '../../core/services/card';
import { Auth } from '../../core/services/auth';

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
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(Auth);

  public payments: any[] = [];
  public cards: any[] = [];
  public isLoading = true;

  // Payment Categories
  public categories = [
    { id: 'UPI', name: 'Transfer to UPI ID', icon: 'currency_rupee', label: 'Recipient UPI ID', placeholder: 'user@upi' },
    { id: 'Mobile Recharge', name: 'Mobile Recharge', icon: 'smartphone', label: 'Mobile Number', placeholder: '9876543210' },
    { id: 'DTH Recharge', name: 'DTH Recharge', icon: 'tv', label: 'DTH Account Number', placeholder: 'DTH12345678' },
    { id: 'Rent', name: 'Rent Payment', icon: 'home', label: 'Landlord Details', placeholder: 'landlord@upi' },
    { id: 'Water Bill', name: 'Water Bill', icon: 'water_drop', label: 'Consumer Number', placeholder: 'WAT-9876' },
    { id: 'Electricity Bill', name: 'Electricity Bill', icon: 'bolt', label: 'Consumer Number', placeholder: 'ELEC-1234' },
    { id: 'Credit Card Repayment', name: 'Credit Card Repayment', icon: 'credit_score', label: 'Credit Card Number', placeholder: '4532 XXXX XXXX XXXX' }
  ];

  public selectedCategory = this.categories[0];

  // Form State
  public paymentForm = { amount: null as number | null, recipientIdentifier: '', sourceCardId: '' };
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
    this.isLoading = true;
    this.paymentService.getExternalHistory().subscribe({
      next: (res: any) => {
        let items: any[] = [];
        if (Array.isArray(res)) items = res;
        else if (res?.data && Array.isArray(res.data)) items = res.data;
        this.payments = items;
        this.isLoading = false;
      },
      error: () => {
        this.payments = [];
        this.isLoading = false;
      }
    });
  }

  loadCards() {
    this.cardService.getCards().subscribe({
      next: (cards) => {
        this.cards = cards || [];
        if (this.cards.length > 0) {
          this.paymentForm.sourceCardId = this.cards[0].cardId || this.cards[0].id;
          this.cdr.detectChanges();
        }
      },
      error: (err) => console.error('Error loading cards', err)
    });
  }

  selectCategory(category: any) {
    this.selectedCategory = category;
    this.paymentForm.recipientIdentifier = '';
    this.errorMessage = '';
  }

  isCreditCard(card: any): boolean {
    // Treat everything as a credit card unless explicitly marked Debit
    return card.cardType !== 'Debit' && !card.network?.toLowerCase().includes('debit');
  }

  get availableCards() {
    return this.cards;
  }

  initiatePayment() {
    this.errorMessage = '';
    this.successMessage = '';
    
    if (!this.paymentForm.sourceCardId) {
      this.errorMessage = 'Please select a card to pay from.';
      return;
    }

    if (!this.paymentForm.amount || this.paymentForm.amount <= 0) {
      this.errorMessage = 'Please enter a valid amount.';
      return;
    }

    this.isInitiating = true;
    
    // Step 1: Request OTP
    const userEmail = localStorage.getItem('user_email') || '';
    this.authService.sendMfa({ email: userEmail, purpose: 'Payment' }).subscribe({
      next: () => {
        this.isInitiating = false;
        this.pendingTransactionId = 'pending'; // Trigger the OTP modal
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to send OTP. Please try again.';
        this.isInitiating = false;
        this.cdr.detectChanges();
      }
    });
  }

  verifyPayment() {
    if (!this.otpCode) {
      this.errorMessage = 'Please enter the 6-digit OTP.';
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Step 2: Verify OTP and Process Payment
    const payload = {
      billType: this.selectedCategory.id,
      amount: this.paymentForm.amount!,
      accountNumber: this.paymentForm.recipientIdentifier,
      cardId: this.paymentForm.sourceCardId,
      otpCode: this.otpCode
    };

    this.paymentService.payExternalBill(payload).subscribe({
      next: (res: any) => {
        const msg = res?.message || res?.data?.message || 'Payment completed successfully!';
        this.successMessage = msg;
        this.isProcessing = false;
        
        // Reset form and close modal
        this.pendingTransactionId = '';
        this.otpCode = '';
        this.paymentForm.amount = null;
        this.paymentForm.recipientIdentifier = '';
        this.loadHistory();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || err.error?.data?.message || 'Payment failed. Please check the OTP and try again.';
        this.isProcessing = false;
        this.cdr.detectChanges();
      }
    });
  }

  cancelPayment() {
    this.pendingTransactionId = '';
    this.otpCode = '';
  }

}

