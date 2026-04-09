import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private authService = inject(Auth);
  private router = inject(Router);

  public registerForm = { firstName: '', lastName: '', email: '', password: '' };
  public isLoading = false;
  public errorMessage = '';

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.register(this.registerForm).subscribe({
      next: () => {
        // Successful registration -> go to OTP verification or login
        this.isLoading = false;
        // Pass the email as state so the OTP page knows who to verify
        this.router.navigate(['/auth/verify-otp'], { state: { email: this.registerForm.email } });
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
        this.isLoading = false;
      }
    });
  }
}
