import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../core/services/support';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './support.html',
  styleUrl: './support.css'
})
export class Support implements OnInit {
  private supportService = inject(SupportService);
  private cdr = inject(ChangeDetectorRef);

  public tickets: any[] = [];
  public isLoading = true;

  public ticketForm = { subject: '', message: '' };
  public isSubmitting = false;

  public errorMessage = '';
  public successMessage = '';

  constructor() { }

  ngOnInit() {
    this.loadTickets();
  }

  public clearFeedback() {
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
  }

  loadTickets() {
    this.isLoading = true;
    this.supportService.getTickets().pipe(
      timeout(10000),
      catchError(err => {
        console.error('Failed to load tickets:', err);
        return of([]);
      })
    ).subscribe({
      next: (data) => {
        this.tickets = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  submitTicket() {
    if (!this.ticketForm.subject || !this.ticketForm.message) {
      this.errorMessage = 'Please provide both a subject and a message.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    this.supportService.createTicket(this.ticketForm).subscribe({
      next: (ticket) => {
        this.successMessage = 'Support ticket #'+ticket.id.slice(0,8)+' submitted successfully.';
        this.tickets.unshift(ticket);
        this.ticketForm = { subject: '', message: '' };
        this.isSubmitting = false;
        this.cdr.detectChanges();
        
        // Auto-clear success message after 5 seconds
        setTimeout(() => this.clearFeedback(), 5000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to submit ticket. Please try again.';
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }
}
