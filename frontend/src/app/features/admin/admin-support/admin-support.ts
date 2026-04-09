import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../core/services/support';
import { SupportTicket } from '../../../core/interfaces/api.interfaces';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-admin-support',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-support.html',
  styleUrl: './admin-support.css'
})
export class AdminSupport implements OnInit {
  private supportService = inject(SupportService);
  private cdr = inject(ChangeDetectorRef);

  public tickets: SupportTicket[] = [];
  public isLoading = true;
  public errorMessage = '';

  public selectedTicket: SupportTicket | null = null;
  public resolutionComment = '';
  public isSubmitting = false;

  ngOnInit() {
    this.loadAdminTickets();
  }

  loadAdminTickets() {
    this.isLoading = true;
    this.supportService.getAdminTickets().pipe(
      timeout(10000),
      catchError(err => {
        console.error('Failed to load admin tickets:', err);
        this.errorMessage = 'Failed to load support tickets.';
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

  openResolveModal(ticket: SupportTicket) {
    this.selectedTicket = ticket;
    this.resolutionComment = '';
  }

  closeModal() {
    this.selectedTicket = null;
  }

  resolveTicket() {
    if (!this.selectedTicket || !this.resolutionComment) return;

    this.isSubmitting = true;
    this.supportService.resolveTicket(this.selectedTicket.id, this.resolutionComment).subscribe({
      next: () => {
        if (this.selectedTicket) {
          this.selectedTicket.status = 'Resolved';
          this.selectedTicket.adminComment = this.resolutionComment;
        }
        this.isSubmitting = false;
        this.selectedTicket = null;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to resolve ticket.';
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }
}
