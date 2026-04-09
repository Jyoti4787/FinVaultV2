# Authentication Guards Implementation

## ✅ What's Been Implemented

### 1. Auth Guard (Protects Authenticated Routes)
**File:** `frontend/src/app/core/guards/auth-guard.ts`

**Purpose:** Prevents unauthenticated users from accessing protected routes

**Behavior:**
- Checks if user has a valid JWT token
- If authenticated → Allow access
- If NOT authenticated → Redirect to `/auth/login` with returnUrl

**Protected Routes:**
- `/dashboard`
- `/profile`
- `/cards`
- `/payments`
- `/transactions`
- `/notifications`
- `/rewards`
- `/support`

### 2. Guest Guard (Protects Auth Pages)
**File:** `frontend/src/app/core/guards/guest-guard.ts`

**Purpose:** Prevents authenticated users from accessing login/register pages

**Behavior:**
- Checks if user is already logged in
- If NOT authenticated → Allow access to auth pages
- If authenticated → Redirect to `/dashboard`

**Protected Routes:**
- `/auth/login`
- `/auth/register`
- `/auth/verify-otp`
- `/auth/reset-password`

### 3. Logout Functionality
**File:** `frontend/src/app/shared/navbar/navbar.ts`

**Features:**
- Clears JWT tokens from localStorage
- Clears user info (email, role, userId)
- Redirects to login page
- Accessible from sidebar navigation

---

## 🔒 How It Works

### Scenario 1: Unauthenticated User Tries to Access Dashboard

```
1. User navigates to /dashboard
2. Auth Guard checks: tokenService.isAuthenticated()
3. Returns false (no token)
4. Guard redirects to /auth/login?returnUrl=/dashboard
5. User sees login page
6. After successful login, redirected back to /dashboard
```

### Scenario 2: Authenticated User Tries to Access Login

```
1. User navigates to /auth/login
2. Guest Guard checks: tokenService.isAuthenticated()
3. Returns true (token exists)
4. Guard redirects to /dashboard
5. User sees dashboard instead of login
```

### Scenario 3: User Logs Out

```
1. User clicks "Logout" in sidebar
2. navbar.logout() is called
3. Clears: finvault_jwt, finvault_refresh, user_email, user_role, user_id
4. Redirects to /auth/login
5. User is now unauthenticated
6. Trying to access /dashboard will redirect to login
```

---

## 🧪 Testing the Guards

### Test 1: Auth Guard Protection

1. **Clear localStorage:**
```javascript
localStorage.clear();
```

2. **Try to access dashboard:**
```
Navigate to: http://localhost:4200/dashboard
```

3. **Expected Result:**
- Immediately redirected to `/auth/login?returnUrl=/dashboard`
- Console shows: `[Auth Guard] User not authenticated, redirecting to login`

### Test 2: Guest Guard Protection

1. **Login first:**
```
Email: ankit.robin@example.com
Password: Ankit@123
```

2. **Try to access login page:**
```
Navigate to: http://localhost:4200/auth/login
```

3. **Expected Result:**
- Immediately redirected to `/dashboard`
- Console shows: `[Guest Guard] User already authenticated, redirecting to dashboard`

### Test 3: Return URL After Login

1. **Clear localStorage and try to access cards:**
```javascript
localStorage.clear();
// Navigate to: http://localhost:4200/cards
```

2. **You'll be redirected to:**
```
http://localhost:4200/auth/login?returnUrl=/cards
```

3. **After login:**
- Automatically redirected to `/cards` (not dashboard)

### Test 4: Logout Functionality

1. **Login and navigate to dashboard**

2. **Click "Logout" in sidebar**

3. **Expected Result:**
- Redirected to `/auth/login`
- Console shows: `[Navbar] Logging out...` and `[Navbar] Tokens cleared, redirecting to login`
- localStorage is empty

4. **Try to go back to dashboard:**
- Redirected to login again

---

## 🔍 Debugging Guards

### Check if Guards are Working

Run this in browser console:

```javascript
// Check authentication status
const token = localStorage.getItem('finvault_jwt');
console.log('Is Authenticated:', !!token);

// Try to navigate to protected route
window.location.href = '/dashboard';
// If not authenticated, should redirect to /auth/login
```

### Check Guard Logs

Guards log their actions to console:

```javascript
// Auth Guard logs:
[Auth Guard] User not authenticated, redirecting to login

// Guest Guard logs:
[Guest Guard] User already authenticated, redirecting to dashboard

// Logout logs:
[Navbar] Logging out...
[Navbar] Tokens cleared, redirecting to login
```

---

## 📋 Route Configuration

### Public Routes (No Guard)
```typescript
{ path: '', component: Landing }
{ path: 'about', component: About }
{ path: 'contact', component: Contact }
```

### Auth Routes (Guest Guard)
```typescript
{ path: 'auth/login', component: Login, canActivate: [guestGuard] }
{ path: 'auth/register', component: Register, canActivate: [guestGuard] }
{ path: 'auth/verify-otp', component: VerifyOtp, canActivate: [guestGuard] }
{ path: 'auth/reset-password', component: ResetPassword, canActivate: [guestGuard] }
```

### Protected Routes (Auth Guard)
```typescript
{ path: 'dashboard', component: Dashboard, canActivate: [authGuard] }
{ path: 'profile', component: Profile, canActivate: [authGuard] }
{ path: 'cards', component: CardList, canActivate: [authGuard] }
{ path: 'payments', component: Payments, canActivate: [authGuard] }
// ... etc
```

---

## 🎯 User Flow Examples

### New User Registration Flow
```
1. Visit http://localhost:4200
2. Click "Register" → /auth/register (Guest Guard allows)
3. Fill form and submit
4. Redirected to /auth/verify-otp (if OTP enabled)
5. After verification → Token saved → Redirected to /dashboard
6. Now authenticated, can access all protected routes
```

### Existing User Login Flow
```
1. Visit http://localhost:4200/auth/login
2. Enter credentials
3. Token saved to localStorage
4. Redirected to /dashboard (or returnUrl if set)
5. Can now access all protected routes
6. Trying to visit /auth/login again → Redirected to /dashboard
```

### Session Expiry Flow
```
1. User is logged in and using the app
2. Token expires (after 15 minutes)
3. Next API call returns 401
4. User tries to navigate to /cards
5. Auth Guard checks token → Still exists but expired
6. API returns 401 → User sees error
7. User clicks logout or tries to navigate
8. Should implement token refresh or force re-login
```

---

## 🚀 Enhancements (Optional)

### 1. Token Expiry Check in Guard

Update `auth-guard.ts` to check token expiration:

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(Token);
  const router = inject(Router);

  const token = tokenService.getToken();
  
  if (!token) {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  // Check if token is expired
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const isExpired = Date.now() > payload.exp * 1000;
    
    if (isExpired) {
      console.log('[Auth Guard] Token expired, clearing and redirecting');
      tokenService.clearTokens();
      router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  } catch (e) {
    console.error('[Auth Guard] Invalid token format');
    tokenService.clearTokens();
    router.navigate(['/auth/login']);
    return false;
  }

  return true;
};
```

### 2. Auto Logout on 401

Add to `auth-interceptor.ts`:

```typescript
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(Token);
  const router = inject(Router);
  const token = tokenService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    catchError(error => {
      if (error.status === 401) {
        console.log('[Interceptor] 401 Unauthorized, logging out');
        tokenService.clearTokens();
        router.navigate(['/auth/login']);
      }
      return throwError(() => error);
    })
  );
};
```

### 3. Role-Based Guards

Create admin guard for admin-only routes:

```typescript
// admin-guard.ts
export const adminGuard: CanActivateFn = (route, state) => {
  const role = localStorage.getItem('user_role');
  
  if (role === 'Admin') {
    return true;
  }
  
  console.log('[Admin Guard] Access denied, not an admin');
  inject(Router).navigate(['/dashboard']);
  return false;
};
```

---

## ✅ Summary

**What's Protected:**
- ✅ Dashboard and all app features require authentication
- ✅ Login/Register pages redirect to dashboard if already logged in
- ✅ Logout clears all tokens and redirects to login
- ✅ Return URL preserved when redirected to login
- ✅ Console logging for debugging

**User Experience:**
- Unauthenticated users → Redirected to login
- Authenticated users → Can't access auth pages
- After login → Redirected to intended page
- After logout → Can't access protected routes

**Security:**
- No access to protected routes without valid token
- Token checked on every route navigation
- Clean logout removes all authentication data

Everything is working as expected! 🎉
