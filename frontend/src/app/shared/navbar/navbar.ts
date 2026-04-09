import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Token } from '../../core/services/token';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  private router = inject(Router);
  private tokenService = inject(Token);

  get isAdmin(): boolean {
    return this.tokenService.isAdmin();
  }

  logout() {
    this.tokenService.clearTokens();
    localStorage.removeItem('user_email');
    localStorage.removeItem('user_role');
    localStorage.removeItem('user_id');
    this.router.navigate(['/auth/login']);
  }
}
