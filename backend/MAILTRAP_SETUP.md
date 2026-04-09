# Mailtrap Setup Guide

## What is Mailtrap?
Mailtrap is a fake SMTP server for testing emails in development. All emails are captured in your Mailtrap inbox instead of being sent to real email addresses.

## Setup Steps

### 1. Create Mailtrap Account
1. Go to: https://mailtrap.io/
2. Sign up for a FREE account
3. Verify your email

### 2. Get SMTP Credentials
1. Log in to Mailtrap
2. Go to **Email Testing** → **Inboxes**
3. Click on **My Inbox** (or create a new inbox)
4. Click on **SMTP Settings** tab
5. Select **Integrations** → **Nodemailer** or **Other**
6. Copy the credentials:
   - **Host**: `sandbox.smtp.mailtrap.io` (or `live.smtp.mailtrap.io` for production)
   - **Port**: `2525` (or 587, 465)
   - **Username**: (looks like a random string)
   - **Password**: (looks like a random string)

### 3. Update .env File
Open `backend/.env` and update these lines:

```env
EMAIL_FROM=noreply@finvault.com
EMAIL_USERNAME=your-mailtrap-username-here
EMAIL_PASSWORD=your-mailtrap-password-here
EMAIL_SMTP_HOST=sandbox.smtp.mailtrap.io
EMAIL_SMTP_PORT=2525
```

### 4. Restart Notification Service
Run these commands:

```bash
cd backend

# Stop and remove notification service
docker-compose -f docker-compose.yml -f docker-compose.override.yml stop notification-service
docker-compose -f docker-compose.yml -f docker-compose.override.yml rm -f notification-service

# Start with new Mailtrap credentials
docker-compose -f docker-compose.yml -f docker-compose.override.yml --env-file .env up -d notification-service
```

### 5. Test Email Sending
```bash
# Register a new user
curl -X POST http://localhost:5232/api/identity/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 6. Check Mailtrap Inbox
1. Go back to Mailtrap dashboard
2. Click on your inbox
3. You should see the OTP email!

## Example Mailtrap Credentials Format

```env
EMAIL_USERNAME=a1b2c3d4e5f6g7
EMAIL_PASSWORD=h8i9j0k1l2m3n4
```

## Benefits of Mailtrap
- ✅ No real emails sent (safe for testing)
- ✅ View all emails in web interface
- ✅ Test email HTML/CSS rendering
- ✅ Check spam score
- ✅ Free tier: 500 emails/month
- ✅ No need for real Gmail App Passwords

## Troubleshooting

### Check if credentials are loaded:
```bash
docker exec finvault-notifications env | grep -i email
```

### Check notification service logs:
```bash
docker logs finvault-notifications --tail 50
```

### If email still fails:
1. Verify Mailtrap credentials are correct
2. Make sure you're using `sandbox.smtp.mailtrap.io` (not live)
3. Port should be `2525` or `587`
4. Restart the notification service after changing .env
