import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Token } from '../services/token';

export const authGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(Token);
  const router = inject(Router);

  const token = tokenService.getToken();
  
  if (!token) {
    // No token, redirect to login
    console.log('[Auth Guard] No token found, redirecting to login');
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  // Check if token is expired
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const isExpired = Date.now() > payload.exp * 1000;
    
    if (isExpired) {
      console.log('[Auth Guard] Token expired, clearing and redirecting to login');
      tokenService.clearTokens();
      localStorage.removeItem('user_email');
      localStorage.removeItem('user_role');
      localStorage.removeItem('user_id');
      router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
    
    // Token is valid
    return true;
  } catch (e) {
    // Invalid token format
    console.error('[Auth Guard] Invalid token format, clearing');
    tokenService.clearTokens();
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
};
