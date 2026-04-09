import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/user';

@Component({
  selector: 'app-profile-picture-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-picture-upload.html',
  styleUrl: './profile-picture-upload.css',
})
export class ProfilePictureUpload {
  private userService = inject(UserService);
  private router = inject(Router);

  public selectedFile: File | null = null;
  public preview: string | null = null;
  public isUploading = false;
  public errorMessage = '';
  public successMessage = '';
  public isDragging = false;

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.processFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave() {
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files[0];
    if (file) this.processFile(file);
  }

  processFile(file: File) {
    if (!file.type.startsWith('image/')) {
      this.errorMessage = 'Only image files are allowed.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage = 'File size must be less than 5MB.';
      return;
    }
    this.selectedFile = file;
    this.errorMessage = '';
    const reader = new FileReader();
    reader.onload = (e) => this.preview = e.target?.result as string;
    reader.readAsDataURL(file);
  }

  upload() {
    if (!this.selectedFile) return;
    this.isUploading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target?.result as string;
      this.userService.updateProfile({ avatarBase64: base64 }).subscribe({
        next: () => {
          this.successMessage = 'Profile picture updated successfully!';
          this.isUploading = false;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to upload profile picture.';
          this.isUploading = false;
        }
      });
    };
    reader.readAsDataURL(this.selectedFile);
  }

  cancel() {
    this.router.navigate(['/profile']);
  }
}
