import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification';
import { timeout, catchError, of } from 'rxjs';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications implements OnInit {
  private notificationService = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  public notifications: any[] = [];
  public isLoading = true;
  public errorMessage = '';

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.isLoading = true;
    this.notificationService.getNotifications().pipe(
      timeout(10000),
      catchError(err => {
        console.error('Notification loading failed:', err);
        return of([]);
      })
    ).subscribe({
      next: (data) => {
        this.notifications = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to load notifications.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getIconForType(type: string): string {
    switch (type?.toLowerCase()) {
      case 'payment':
      case 'transaction':
        return 'sync_alt';
      case 'security':
      case 'auth':
      case 'otp':
        return 'security';
      case 'system':
        return 'info';
      default:
        return 'notifications';
    }
  }
}
