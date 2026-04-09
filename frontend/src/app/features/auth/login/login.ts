import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../../core/services/auth';
import { Token } from '../../../core/services/token';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(Auth);
  private tokenService = inject(Token);

  public email = '';
  public password = '';
  public errorMessage = '';
  public isLoading = false;
  private returnUrl: string = '/dashboard';

  ngOnInit() {
    // Get the return URL from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter both email and password.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login({ email: this.email, password: this.password, correlationId: '00000000-0000-0000-0000-000000000000' }).subscribe({
      next: (res) => {
        console.log('Login response:', res);
        console.log('Access token:', res.accessToken);
        
        this.isLoading = false;
        
        if (res.requiresOtp) {
          // If OTP required, you could store email in a service or LocalStorage and nav to verify-otp
          localStorage.setItem('pending_verification_email', this.email);
          this.router.navigate(['/auth/verify-otp']);
        } else if (res.accessToken) {
          console.log('Saving token to localStorage...');
          this.tokenService.setTokens(res.accessToken, res.refreshToken);
          
          const savedToken = localStorage.getItem('finvault_jwt');
          console.log('Token saved successfully:', !!savedToken);
          
          // Store user info
          localStorage.setItem('user_email', res.email);
          localStorage.setItem('user_role', res.role);
          localStorage.setItem('user_id', res.userId);
          
          // Route admin to admin dashboard, regular users to user dashboard
          const destination = res.role === 'Admin' ? '/admin/cards' : this.returnUrl;
          console.log('Navigating to:', destination, ' (role:', res.role, ')');
          this.router.navigateByUrl(destination);
        } else {
          console.error('No accessToken in response:', res);
          this.errorMessage = 'Login failed: No access token received';
        }
      },
      error: (err) => {
        console.error('Login error:', err);
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }
}
