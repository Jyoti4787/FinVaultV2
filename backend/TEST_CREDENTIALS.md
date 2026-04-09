# FinVault Test Credentials

## Regular User Account
- **Email:** `ankit.robin@example.com`
- **Password:** `Ankit@123`
- **Role:** User
- **User ID:** `95e971ac-b77c-4e00-9869-ad4544e1bf57`

### User JWT Token (expires in future)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5NWU5NzFhYy1iNzdjLTRlMDAtOTg2OS1hZDQ1NDRlMWJmNTciLCJlbWFpbCI6ImFua2l0LnJvYmluQGV4YW1wbGUuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImp0aSI6ImFhOTk1Y2Y1LTVlNDItNDY1My1iOTk2LTVmODE4NGNmMmZhMyIsImV4cCI6MTc3NTY1MzQxNCwiaXNzIjoiaHR0cHM6Ly9maW52YXVsdC5pbyIsImF1ZCI6ImZpbnZhdWx0LWlkZW50aXR5In0.AdGTYeyzX4Tpn83zkMMOs5rpElcpU1OACmCC_G0Ysug
```

### User Test Data
- **Cards:** 2 credit cards (Visa and Mastercard)
- **Payments:** 3 payment transactions
- **Notifications:** 4 notifications (2 read, 2 unread)

---

## Admin Account
- **Email:** `admin@finvault.io`
- **Password:** `Admin@123`
- **Role:** Admin
- **User ID:** `9f3984c4-96a7-4249-b9e2-6f1318ca0a29`

### Admin JWT Token (expires in future)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ZjM5ODRjNC05NmE3LTQyNDktYjllMi02ZjEzMThjYTBhMjkiLCJlbWFpbCI6ImFkbWluQGZpbnZhdWx0LmlvIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJqdGkiOiI2M2YzNTI3OS01YzA3LTRjZDctYWVmMC0zYTUwYTUzN2QxZDQiLCJleHAiOjE3NzU2NTM5NTMsImlzcyI6Imh0dHBzOi8vZmludmF1bHQuaW8iLCJhdWQiOiJmaW52YXVsdC1pZGVudGl0eSJ9.ecuGs6Dw4F69ENurdYaZRPyU1vqblZBc5ttmNUPpRO0
```

---

## How to Use in Swagger UI

1. **Open Swagger:** `http://localhost:5001/swagger`

2. **Click "Authorize"** button (🔒 lock icon at top right)

3. **Enter token with "Bearer" prefix:**
   - For User endpoints: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5NWU5NzFhYy1iNzdjLTRlMDAtOTg2OS1hZDQ1NDRlMWJmNTciLCJlbWFpbCI6ImFua2l0LnJvYmluQGV4YW1wbGUuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImp0aSI6ImFhOTk1Y2Y1LTVlNDItNDY1My1iOTk2LTVmODE4NGNmMmZhMyIsImV4cCI6MTc3NTY1MzQxNCwiaXNzIjoiaHR0cHM6Ly9maW52YXVsdC5pbyIsImF1ZCI6ImZpbnZhdWx0LWlkZW50aXR5In0.AdGTYeyzX4Tpn83zkMMOs5rpElcpU1OACmCC_G0Ysug`
   - For Admin endpoints: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ZjM5ODRjNC05NmE3LTQyNDktYjllMi02ZjEzMThjYTBhMjkiLCJlbWFpbCI6ImFkbWluQGZpbnZhdWx0LmlvIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJqdGkiOiI2M2YzNTI3OS01YzA3LTRjZDctYWVmMC0zYTUwYTUzN2QxZDQiLCJleHAiOjE3NzU2NTM5NTMsImlzcyI6Imh0dHBzOi8vZmludmF1bHQuaW8iLCJhdWQiOiJmaW52YXVsdC1pZGVudGl0eSJ9.ecuGs6Dw4F69ENurdYaZRPyU1vqblZBc5ttmNUPpRO0`

4. **Click "Authorize"** then **"Close"**

5. **Test endpoints** - they will now include the Authorization header automatically

---

## Using cURL

### User Endpoints Example
```bash
curl -X GET "http://localhost:5001/api/cards" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5NWU5NzFhYy1iNzdjLTRlMDAtOTg2OS1hZDQ1NDRlMWJmNTciLCJlbWFpbCI6ImFua2l0LnJvYmluQGV4YW1wbGUuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImp0aSI6ImFhOTk1Y2Y1LTVlNDItNDY1My1iOTk2LTVmODE4NGNmMmZhMyIsImV4cCI6MTc3NTY1MzQxNCwiaXNzIjoiaHR0cHM6Ly9maW52YXVsdC5pbyIsImF1ZCI6ImZpbnZhdWx0LWlkZW50aXR5In0.AdGTYeyzX4Tpn83zkMMOs5rpElcpU1OACmCC_G0Ysug"
```

### Admin Endpoints Example
```bash
curl -X GET "http://localhost:5001/api/identity/users" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ZjM5ODRjNC05NmE3LTQyNDktYjllMi02ZjEzMThjYTBhMjkiLCJlbWFpbCI6ImFkbWluQGZpbnZhdWx0LmlvIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJqdGkiOiI2M2YzNTI3OS01YzA3LTRjZDctYWVmMC0zYTUwYTUzN2QxZDQiLCJleHAiOjE3NzU2NTM5NTMsImlzcyI6Imh0dHBzOi8vZmludmF1bHQuaW8iLCJhdWQiOiJmaW52YXVsdC1pZGVudGl0eSJ9.ecuGs6Dw4F69ENurdYaZRPyU1vqblZBc5ttmNUPpRO0"
```

---

## Important Notes

1. **Always include "Bearer " prefix** when using tokens in Swagger UI or cURL
2. **Tokens expire after 15 minutes** - login again to get a new token if expired
3. **Admin endpoints** require Admin role - they will return 403 Forbidden for regular users
4. **User endpoints** work for both User and Admin roles
5. **Development mode** - OTP verification is disabled for easier testing

---

## Available Services

- **Gateway (Aggregated Swagger):** http://localhost:5001/swagger
- **Identity Service:** http://localhost:5232/swagger
- **Card Service:** http://localhost:5121/swagger
- **Payment Service:** http://localhost:5181/swagger
- **Notification Service:** http://localhost:5191/swagger
