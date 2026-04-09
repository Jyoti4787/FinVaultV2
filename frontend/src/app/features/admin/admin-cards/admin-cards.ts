import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardService } from '../../../core/services/card';
import { Card } from '../../../core/interfaces/api.interfaces';

@Component({
  selector: 'app-admin-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-cards.html',
  styleUrl: './admin-cards.css'
})
export class AdminCardsComponent implements OnInit {
  private cardService = inject(CardService);
  private cdr = inject(ChangeDetectorRef);
  
  public cards: Card[] = [];
  public isLoading = true;
  public errorMessage = '';

  ngOnInit() {
    this.loadAdminCards();
  }

  loadAdminCards() {
    this.isLoading = true;
    this.errorMessage = '';
    this.cardService.getAdminCards().subscribe({
      next: (cards: Card[]) => {
        this.cards = cards || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to load user cards. Verify you have Admin access.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  approveCard(cardId: string) {
    this.cardService.approveAdminCard(cardId).subscribe({
      next: () => {
        const card = this.cards.find(c => c.cardId === cardId);
        if (card) {
          card.isVerified = true;
        }
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to approve card.';
        this.cdr.detectChanges();
      }
    });
  }
}
