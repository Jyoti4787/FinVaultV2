import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionService } from '../../core/services/transaction';
import { Transaction } from '../../core/interfaces/api.interfaces';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions.html',
  styleUrl: './transactions.css'
})
export class Transactions implements OnInit {
  private txService = inject(TransactionService);
  private cdr = inject(ChangeDetectorRef);
  
  public transactions: Transaction[] = [];
  public isLoading = true;
  public errorMessage = '';

  ngOnInit() {
    this.txService.getTransactions().pipe(
      timeout(5000),
      catchError(err => {
        console.error('Transaction loading failed:', err);
        return of([]); // Return empty list on failure
      })
    ).subscribe({
      next: (txs) => {
        this.transactions = txs || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load transactions.';
        this.cdr.detectChanges();
      }
    });
  }
}
