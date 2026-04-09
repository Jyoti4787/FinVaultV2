# FinVault API Endpoints - Authorization Guide

## Authorization Summary

All endpoints in FinVault require authentication via JWT token. Currently, there are **NO role-specific restrictions** - both Admin and User roles can access all endpoints.

### Current Authorization Model
- ✅ **Admin** can access ALL endpoints
- ✅ **User** can access ALL endpoints
- 🔒 All endpoints require valid JWT token with `Authorization: Bearer <token>`

---

## Identity Service Endpoints
**Base URL:** `http://localhost:5001/api/identity`

### Authentication (No Auth Required)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new user | ❌ No |
| POST | `/auth/register/verify` | Verify registration OTP | ❌ No |
| POST | `/auth/login` | Login user | ❌ No |
| POST | `/auth/login/verify-otp` | Verify login OTP | ❌ No |
| POST | `/auth/refresh` | Refresh access token | ❌ No |
| POST | `/auth/forgot-password` | Request password reset | ❌ No |
| POST | `/auth/reset-password` | Reset password with token | ❌ No |
| POST | `/auth/verify-email` | Verify email address | ❌ No |
| POST | `/auth/mfa/send` | Send MFA OTP | ❌ No |
| POST | `/auth/mfa/verify` | Verify MFA OTP | ❌ No |

### User Profile (Auth Required)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/users/profile` | Get current user profile | 👤 User, 👑 Admin |
| GET | `/users/me` | Alias for /profile | 👤 User, 👑 Admin |
| POST | `/users/profile/picture` | Upload profile picture | 👤 User, 👑 Admin |
| GET | `/users/profile/picture/{userId}` | Get profile picture | 🌐 Public (No Auth) |

---

## Card Service Endpoints
**Base URL:** `http://localhost:5001/api/cards`

All card endpoints require authentication and work with the logged-in user's cards.

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get all cards for logged-in user | 👤 User, 👑 Admin |
| GET | `/{cardId}` | Get specific card details | 👤 User, 👑 Admin |
| GET | `/{cardId}/utilization` | Get card utilization % | 👤 User, 👑 Admin |
| GET | `/utilization` | Get total utilization across all cards | 👤 User, 👑 Admin |
| GET | `/{cardId}/reveal` | Reveal full card number & CVV | 👤 User, 👑 Admin |
| POST | `/` | Add new credit card | 👤 User, 👑 Admin |
| DELETE | `/{cardId}` | Remove card (soft delete) | 👤 User, 👑 Admin |
| PATCH | `/{cardId}/default` | Set card as default | 👤 User, 👑 Admin |
| POST | `/{cardId}/verify` | Verify card with ₹1 micro-auth | 👤 User, 👑 Admin |
| PATCH | `/{cardId}/limit` | Update credit limit | 👤 User, 👑 Admin |

---

## Payment Service Endpoints
**Base URL:** `http://localhost:5001/api/payments`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get payment history | 👤 User, 👑 Admin |
| GET | `/history` | Get payment history (alias) | 👤 User, 👑 Admin |
| POST | `/` | Initiate payment | 👤 User, 👑 Admin |
| POST | `/process` | Process payment with OTP | 👤 User, 👑 Admin |
| POST | `/initiate-otp` | Get OTP flow instructions | 👤 User, 👑 Admin |
| PUT | `/{paymentId}/complete` | Complete initiated payment | 👤 User, 👑 Admin |
| PUT | `/{paymentId}/reverse` | Reverse/cancel payment | 👤 User, 👑 Admin |

### Transactions
**Base URL:** `http://localhost:5001/api/transactions`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get all transactions | 👤 User, 👑 Admin |
| GET | `/{transactionId}` | Get transaction details | 👤 User, 👑 Admin |
| POST | `/` | Create new transaction | 👤 User, 👑 Admin |

### External Bills
**Base URL:** `http://localhost:5001/api/external-bills`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get all external bills | 👤 User, 👑 Admin |
| GET | `/{billId}` | Get bill details | 👤 User, 👑 Admin |
| POST | `/` | Add external bill | 👤 User, 👑 Admin |
| PUT | `/{billId}` | Update bill | 👤 User, 👑 Admin |
| DELETE | `/{billId}` | Delete bill | 👤 User, 👑 Admin |

---

## Notification Service Endpoints
**Base URL:** `http://localhost:5001/api/notifications`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get all notifications for user | 👤 User, 👑 Admin |
| POST | `/` | Create notification | 👤 User, 👑 Admin |
| PUT | `/{notificationId}` | Update notification | 👤 User, 👑 Admin |
| DELETE | `/{notificationId}` | Delete notification | 👤 User, 👑 Admin |

### Rewards
**Base URL:** `http://localhost:5001/api/rewards`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get user rewards | 👤 User, 👑 Admin |
| POST | `/` | Create reward | 👤 User, 👑 Admin |

### Support
**Base URL:** `http://localhost:5001/api/support`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/` | Get support tickets | 👤 User, 👑 Admin |
| POST | `/` | Create support ticket | 👤 User, 👑 Admin |

---

## Important Notes

### 1. No Admin-Only Endpoints Currently
Currently, there are **no endpoints that are restricted to Admin role only**. All authenticated endpoints accept both User and Admin roles.

### 2. User Isolation
Even though Admin can access all endpoints, the endpoints use `GetUserId()` from the JWT token, which means:
- Admin sees their own cards, not all users' cards
- Admin sees their own payments, not all users' payments
- Admin sees their own notifications, not all users' notifications

### 3. Recommended Admin Endpoints to Add

To make the Admin role useful, you should add these admin-only endpoints:

**Identity Service - Admin Endpoints (To Be Added)**
```
GET    /api/identity/admin/users              - List all users
GET    /api/identity/admin/users/{userId}     - Get any user's details
PUT    /api/identity/admin/users/{userId}     - Update user details
DELETE /api/identity/admin/users/{userId}     - Delete user
POST   /api/identity/admin/users/{userId}/ban - Ban user
```

**Card Service - Admin Endpoints (To Be Added)**
```
GET    /api/cards/admin/all                   - View all users' cards
GET    /api/cards/admin/user/{userId}         - View specific user's cards
POST   /api/cards/admin/{cardId}/freeze       - Freeze any card
POST   /api/cards/admin/{cardId}/unfreeze     - Unfreeze any card
```

**Payment Service - Admin Endpoints (To Be Added)**
```
GET    /api/payments/admin/all                - View all payments
GET    /api/payments/admin/user/{userId}      - View user's payments
POST   /api/payments/admin/{paymentId}/refund - Refund payment
```

### 4. How to Add Role-Based Authorization

To restrict an endpoint to Admin only, add this attribute:

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("admin/users")]
public async Task<IActionResult> GetAllUsers()
{
    // Admin-only logic
}
```

To allow both User and Admin:

```csharp
[Authorize(Roles = "User,Admin")]
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    // User or Admin can access
}
```

---

## Testing with Swagger

### For User Endpoints
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5NWU5NzFhYy1iNzdjLTRlMDAtOTg2OS1hZDQ1NDRlMWJmNTciLCJlbWFpbCI6ImFua2l0LnJvYmluQGV4YW1wbGUuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImp0aSI6ImFhOTk1Y2Y1LTVlNDItNDY1My1iOTk2LTVmODE4NGNmMmZhMyIsImV4cCI6MTc3NTY1MzQxNCwiaXNzIjoiaHR0cHM6Ly9maW52YXVsdC5pbyIsImF1ZCI6ImZpbnZhdWx0LWlkZW50aXR5In0.AdGTYeyzX4Tpn83zkMMOs5rpElcpU1OACmCC_G0Ysug
```

### For Admin Endpoints
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ZjM5ODRjNC05NmE3LTQyNDktYjllMi02ZjEzMThjYTBhMjkiLCJlbWFpbCI6ImFkbWluQGZpbnZhdWx0LmlvIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJqdGkiOiI2M2YzNTI3OS01YzA3LTRjZDctYWVmMC0zYTUwYTUzN2QxZDQiLCJleHAiOjE3NzU2NTM5NTMsImlzcyI6Imh0dHBzOi8vZmludmF1bHQuaW8iLCJhdWQiOiJmaW52YXVsdC1pZGVudGl0eSJ9.ecuGs6Dw4F69ENurdYaZRPyU1vqblZBc5ttmNUPpRO0
```

---

## Summary

✅ **Admin can access all endpoints** - there are no restrictions  
✅ **User can access all endpoints** - same as Admin  
⚠️ **No admin-specific functionality** - Admin sees only their own data  
💡 **Recommendation:** Add admin-only endpoints for user management, monitoring, and system administration
