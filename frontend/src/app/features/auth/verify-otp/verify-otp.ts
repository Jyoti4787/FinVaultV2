import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../../../core/services/auth';
import { Token } from '../../../core/services/token';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verify-otp.html',
  styleUrl: './verify-otp.css',
})
export class VerifyOtp implements OnInit {
  private authService = inject(Auth);
  private tokenService = inject(Token);
  private router = inject(Router);

  public email = '';
  public otp = '';
  public isLoading = false;
  public errorMessage = '';

  ngOnInit() {
    // If we passed email via router state from Login/Register
    const navParams = this.router.getCurrentNavigation()?.extras.state;
    if (navParams && navParams['email']) {
      this.email = navParams['email'];
    }
  }

  onSubmit() {
    if (!this.email || !this.otp) {
      this.errorMessage = 'Please enter both your email and the OTP code.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verifyOtp({ email: this.email, otp: this.otp }).subscribe({
      next: (res) => {
        // Save the real token
        this.tokenService.setTokens(res.accessToken, res.refreshToken);
        // Store user info
        localStorage.setItem('user_email', res.email);
        localStorage.setItem('user_role', res.role);
        localStorage.setItem('user_id', res.userId);

        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Invalid OTP. Please try again.';
        this.isLoading = false;
      }
    });
  }
}
