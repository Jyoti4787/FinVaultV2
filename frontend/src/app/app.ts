import { Component, signal, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { Navbar } from './shared/navbar/navbar';
import { CommonModule } from '@angular/common';
import { UserService } from './core/services/user';
import { Token } from './core/services/token';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, CommonModule],
  template: `
    <ng-container *ngIf="!isAuthRoute(); else authLayout">
      <div class="app-container">
        <aside class="sidebar-wrapper">
          <app-navbar></app-navbar>
        </aside>
        
        <main class="main-content">
          <header class="top-header glass-panel">
            <div class="header-search">
              <span class="material-icons-outlined">search</span>
              <input type="text" placeholder="Search transactions, cards..." />
            </div>
            <div class="header-actions">
              <button class="icon-btn"><span class="material-icons-outlined">notifications</span></button>
              <div class="user-avatar">{{ userInitial() }}</div>
            </div>
          </header>
          
          <div class="content-area">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
    </ng-container>
    
    <ng-template #authLayout>
      <router-outlet></router-outlet>
    </ng-template>
  `,
  styleUrl: './app.css'
})
export class App implements OnInit {
  private router = inject(Router);
  private userService = inject(UserService);
  private tokenSvc = inject(Token);
  private cdr = inject(ChangeDetectorRef);
  
  protected readonly title = signal('frontend');
  userInitial = signal('');
  
  ngOnInit() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      if (this.tokenSvc.isAuthenticated() && !this.isAuthRoute() && !this.userInitial()) {
        this.loadUserInitial();
      }
    });

    if (this.tokenSvc.isAuthenticated() && !this.isAuthRoute()) {
      this.loadUserInitial();
    }
  }

  loadUserInitial() {
    this.userService.getProfile().subscribe({
      next: (user) => {
        if (user && user.firstName) {
          this.userInitial.set(user.firstName.charAt(0).toUpperCase());
          this.cdr.detectChanges();
        } else if (user && user.email) {
          this.userInitial.set(user.email.charAt(0).toUpperCase());
          this.cdr.detectChanges();
        } else {
          this.userInitial.set('U');
          this.cdr.detectChanges();
        }
      },
      error: () => {}
    });
  }
  
  isAuthRoute(): boolean {
    const url = this.router.url;
    return url.includes('/auth') || url === '/' || url === '/about' || url === '/contact' || url.startsWith('/about') || url.startsWith('/contact');
  }
}
