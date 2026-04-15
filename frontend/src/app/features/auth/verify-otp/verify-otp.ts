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
  public isResending = false;
  public resendMessage = '';

  ngOnInit() {
    // Try to get email from router state first, then from localStorage
    const navParams = this.router.getCurrentNavigation()?.extras.state;
    if (navParams && navParams['email']) {
      this.email = navParams['email'];
    } else {
      // Fallback: get from localStorage (set by login or register page)
      this.email = localStorage.getItem('pending_verification_email') || '';
    }
  }

  onSubmit() {
    if (!this.email || !this.otp) {
      this.errorMessage = 'Please enter both your email and the OTP code.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verifyOtp({ email: this.email, otpCode: this.otp }).subscribe({
      next: (res) => {
        // Save the real token
        this.tokenService.setTokens(res.accessToken, res.refreshToken);
        // Store user info
        localStorage.setItem('user_email', res.email);
        localStorage.setItem('user_role', res.role);
        localStorage.setItem('user_id', res.userId);

        // Clean up pending state
        localStorage.removeItem('pending_verification_email');
        
        this.isLoading = false;

        // Route based on role
        const returnUrl = localStorage.getItem('pending_return_url') || '/dashboard';
        localStorage.removeItem('pending_return_url');

        const destination = res.role === 'Admin' ? '/admin/cards' : returnUrl;
        this.router.navigateByUrl(destination);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Invalid OTP. Please try again.';
        this.isLoading = false;
      }
    });
  }

  resendOtp() {
    if (!this.email) {
      this.errorMessage = 'Please enter your email address first.';
      return;
    }

    this.isResending = true;
    this.resendMessage = '';
    this.errorMessage = '';

    this.authService.sendMfa({ email: this.email }).subscribe({
      next: () => {
        this.isResending = false;
        this.resendMessage = 'A new code has been sent to your email.';
        setTimeout(() => this.resendMessage = '', 5000);
      },
      error: (err) => {
        this.isResending = false;
        this.errorMessage = err.error?.message || 'Failed to resend OTP. Please try again.';
      }
    });
  }
}
