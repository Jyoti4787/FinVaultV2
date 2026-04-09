import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { CommonModule, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CardService } from '../../core/services/card';
import { TransactionService } from '../../core/services/transaction';
import { Card, Transaction, User } from '../../core/interfaces/api.interfaces';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserService } from '../../core/services/user';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [BaseChartDirective, CommonModule, DecimalPipe, RouterLink],
  providers: [provideCharts(withDefaultRegisterables())],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  private cardService = inject(CardService);
  private txService = inject(TransactionService);
  private userService = inject(UserService);
  private cdr = inject(ChangeDetectorRef);

  public userName = ''; 
  public totalBalance = 0;
  public totalCards = 0;
  
  public recentTransactions: Transaction[] = [];
  public isLoading = true;

  // Chart Configuration
  public barChartLegend = false;
  public barChartPlugins = [];

  public barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [ 'Oct', 'Nov', 'Dec', 'Jan', 'Feb', 'Mar' ],
    datasets: [
      { 
        data: [ 0, 0, 0, 0, 0, 0 ], 
        label: 'Spending',
        backgroundColor: '#2f2a24',
        borderRadius: 4
      }
    ]
  };

  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: { beginAtZero: true, grid: { display: true, color: 'rgba(221, 214, 206, 0.4)' }, border: { display: false } },
      x: { grid: { display: false }, border: { display: false } }
    },
    plugins: { legend: { display: false } }
  };

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;

    // catchError on each stream: if one API fails, return a safe default
    // so forkJoin always completes and the dashboard always renders
    const profile$ = this.userService.getProfile().pipe(
      catchError(() => of(null as unknown as User))
    );
    const cards$ = this.cardService.getCards().pipe(
      catchError(() => of([] as Card[]))
    );
    const transactions$ = this.txService.getTransactions().pipe(
      catchError(() => of([] as Transaction[]))
    );

    forkJoin({ profile: profile$, cards: cards$, transactions: transactions$ }).subscribe({
      next: (data) => {
        if (data.profile) {
          this.userName = data.profile.firstName || 'User';
        }

        const cards = data.cards || [];
        this.recentTransactions = (data.transactions || []).slice(0, 5); 
        
        this.totalCards = cards.length;
        this.totalBalance = cards.reduce(
          (sum: number, card: Card) => sum + (card.creditLimit - card.outstandingBalance), 0
        );
        
        const now = new Date();
        const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const labels: string[] = [];
        const monthData: number[] = [0, 0, 0, 0, 0, 0];
        
        for (let i = 5; i >= 0; i--) {
          const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
          labels.push(monthNames[d.getMonth()]);
        }
        
        this.barChartData = { ...this.barChartData, labels };

        (data.transactions || []).forEach(tx => {
          const txDate = new Date(tx.timestamp); 
          const diffMonths = (now.getFullYear() - txDate.getFullYear()) * 12 + now.getMonth() - txDate.getMonth();
          if (diffMonths >= 0 && diffMonths < 6) {
             if (tx.type?.toLowerCase() === 'debit') {
                monthData[5 - diffMonths] += tx.amount;
             }
          }
        });
        
        this.barChartData = {
          ...this.barChartData,
          datasets: [{ ...this.barChartData.datasets[0], data: monthData }]
        };
        
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Dashboard fatal error', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
