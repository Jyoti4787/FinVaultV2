# Debug Authentication Issue

## Problem
After login, API calls return 401 Unauthorized even though login was successful.

## Steps to Debug

### 1. Check if Token is Stored
Open browser console (F12) after login and run:

```javascript
// Check if token exists
console.log('Token:', localStorage.getItem('finvault_jwt'));

// Check user info
console.log('User Email:', localStorage.getItem('user_email'));
console.log('User Role:', localStorage.getItem('user_role'));
console.log('User ID:', localStorage.getItem('user_id'));
```

**Expected:** Should see a JWT token string

**If null:** Token is not being saved during login

### 2. Check Login Response
In Network tab (F12), find the login request:
1. Look for `POST http://localhost:5001/api/identity/auth/login`
2. Click on it
3. Go to "Response" tab
4. Verify structure:

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "...",
    "userId": "...",
    "email": "...",
    "role": "User"
  }
}
```

### 3. Check if Interceptor is Adding Token
In Network tab, find any API call after login (e.g., GET /api/cards):
1. Click on the request
2. Go to "Headers" tab
3. Look for "Request Headers"
4. Check if `Authorization: Bearer <token>` is present

**If missing:** Interceptor is not working

### 4. Manual Test
After login, run this in console:

```javascript
// Get token
const token = localStorage.getItem('finvault_jwt');
console.log('Token exists:', !!token);

// Test API call with token
fetch('http://localhost:5001/api/cards', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(r => r.json())
.then(data => console.log('Cards:', data))
.catch(err => console.error('Error:', err));
```

**Expected:** Should return cards data

**If 401:** Token is invalid or expired

---

## Common Issues & Fixes

### Issue 1: Token Not Saved
**Symptom:** `localStorage.getItem('finvault_jwt')` returns `null`

**Cause:** Login component not calling `tokenService.setTokens()`

**Fix:** Check `login.ts` line where token is saved:
```typescript
this.tokenService.setTokens(res.accessToken, res.refreshToken);
```

### Issue 2: Wrong Token Field
**Symptom:** Login succeeds but token is undefined

**Cause:** Response has `accessToken` but code looks for `token`

**Fix:** Already fixed in login.ts - uses `res.accessToken`

### Issue 3: Interceptor Not Running
**Symptom:** Token exists but not in request headers

**Cause:** Interceptor not registered or not executing

**Fix:** Verify `app.config.ts` has:
```typescript
provideHttpClient(withInterceptors([authInterceptor]))
```

### Issue 4: Token Expired
**Symptom:** Token exists but API returns 401

**Cause:** JWT tokens expire after 15 minutes

**Fix:** Login again to get fresh token

---

## Quick Fix Script

Run this in browser console after login to diagnose:

```javascript
// Diagnostic Script
console.log('=== Auth Diagnostic ===');

// 1. Check token storage
const token = localStorage.getItem('finvault_jwt');
const refresh = localStorage.getItem('finvault_refresh');
console.log('1. Token stored:', !!token);
console.log('   Token length:', token?.length || 0);
console.log('   Refresh token:', !!refresh);

// 2. Decode JWT (without verification)
if (token) {
  try {
    const parts = token.split('.');
    const payload = JSON.parse(atob(parts[1]));
    console.log('2. Token payload:', payload);
    console.log('   User ID:', payload.sub);
    console.log('   Email:', payload.email);
    console.log('   Role:', payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']);
    console.log('   Expires:', new Date(payload.exp * 1000));
    console.log('   Is expired:', Date.now() > payload.exp * 1000);
  } catch (e) {
    console.error('   Failed to decode token:', e);
  }
}

// 3. Test API call
if (token) {
  console.log('3. Testing API call...');
  fetch('http://localhost:5001/api/cards', {
    headers: { 'Authorization': `Bearer ${token}` }
  })
  .then(r => {
    console.log('   Status:', r.status);
    return r.json();
  })
  .then(data => console.log('   Response:', data))
  .catch(err => console.error('   Error:', err));
}

console.log('=== End Diagnostic ===');
```

---

## Solution Steps

If token is not being saved:

1. **Clear browser storage:**
```javascript
localStorage.clear();
```

2. **Rebuild frontend:**
```bash
cd frontend
npm run build
```

3. **Restart dev server:**
```bash
npm start
```

4. **Login again** and check console

---

## Expected Flow

1. User enters credentials
2. POST /api/identity/auth/login
3. Backend returns `{ success: true, data: { accessToken: "...", ... } }`
4. Auth service unwraps to `{ accessToken: "...", ... }`
5. Login component calls `tokenService.setTokens(res.accessToken, res.refreshToken)`
6. Token stored in localStorage as `finvault_jwt`
7. User navigates to dashboard
8. Dashboard loads cards
9. GET /api/cards
10. Auth interceptor adds `Authorization: Bearer <token>` header
11. Backend validates token
12. Returns cards data

If any step fails, the 401 error occurs.
