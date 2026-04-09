import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Token } from '../services/token';

/**
 * Guest Guard - Prevents authenticated users from accessing auth pages
 * Redirects to dashboard if already logged in with valid token
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(Token);
  const router = inject(Router);

  const token = tokenService.getToken();
  
  if (!token) {
    // No token, allow access to auth pages
    return true;
  }

  // Check if token is valid and not expired
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const isExpired = Date.now() > payload.exp * 1000;
    
    if (isExpired) {
      // Token expired, clear it and allow access to auth pages
      console.log('[Guest Guard] Token expired, clearing');
      tokenService.clearTokens();
      return true;
    }
    
    // Token is valid, redirect to dashboard
    console.log('[Guest Guard] User already authenticated, redirecting to dashboard');
    router.navigate(['/dashboard']);
    return false;
  } catch (e) {
    // Invalid token format, clear it and allow access
    console.error('[Guest Guard] Invalid token, clearing');
    tokenService.clearTokens();
    return true;
  }
};
