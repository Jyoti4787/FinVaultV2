import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user';
import { User } from '../../core/interfaces/api.interfaces';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private userService = inject(UserService);
  private cdr = inject(ChangeDetectorRef);

  public user: User | null = null;
  public isLoading = true;
  public isUpdating = false;
  public errorMessage = '';
  public successMessage = '';

  public profileForm: Partial<User> = {};

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.isLoading = true;
    this.userService.getProfile().subscribe({
      next: (user) => {
        this.user = user;
        if (user) {
          this.profileForm = {
            firstName: user.firstName,
            lastName: user.lastName,
            phoneNumber: user.phoneNumber
          };
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to load profile.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  updateProfile() {
    this.isUpdating = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    this.userService.updateProfile(this.profileForm).subscribe({
      next: (user) => {
        this.user = user;
        this.successMessage = 'Profile updated successfully!';
        this.isUpdating = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to update profile.';
        this.isUpdating = false;
        this.cdr.detectChanges();
      }
    });
  }
}
