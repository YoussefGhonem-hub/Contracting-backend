# SendGrid Email Troubleshooting Guide

## 🚨 Unauthorized (401) Error - FIX THIS FIRST

If you're getting "Unauthorized" error, it means your **SendGrid API Key is invalid or expired**.

### How to Fix:

1. **Get a New API Key from SendGrid:**
   - Go to https://app.sendgrid.com/
   - Navigate to: **Settings → API Keys**
   - Click **"Create API Key"**
   - Name: `Contracting-API-Key`
   - Permissions: Select **"Full Access"** or at minimum **"Mail Send"**
   - Click **Create & View**
   - **COPY THE KEY IMMEDIATELY** (you can't see it again!)

2. **Update your appsettings.json:**
   ```json
   "SendGridSettings": {
     "ApiKey": "SG.YOUR_NEW_API_KEY_HERE",
     "FromEmail": "noreply@contracting.app",
     "FromName": "Contracting System"
   }
   ```

3. **Verify the API Key format:**
   - Must start with `SG.`
   - Should be around 69 characters long
   - Example: `SG.xxxxxxxxxxxxxxxxxxx.yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy`

---

## ⚠️ Forbidden (403) Error - Sender Not Verified

This means your **FromEmail is not verified** in SendGrid.

### How to Fix:

#### Option 1: Single Sender Verification (Quick but limited)
1. Go to https://app.sendgrid.com/
2. Navigate to: **Settings → Sender Authentication → Verify a Single Sender**
3. Click **"Create New Sender"**
4. Fill in the form:
   - **From Email Address**: `noreply@contracting.app`
   - **From Name**: `Contracting System`
   - Fill in other required fields
5. Click **Create**
6. Check the email inbox for `noreply@contracting.app` 
7. Click the verification link in the email

#### Option 2: Domain Authentication (Recommended for production)
1. Go to https://app.sendgrid.com/
2. Navigate to: **Settings → Sender Authentication → Authenticate Your Domain**
3. Follow the wizard to add DNS records for `contracting.app`
4. After DNS propagation (can take 24-48 hours), any email @contracting.app will work

---

## 🧪 Testing Your Configuration

### Step 1: Check Configuration
```bash
GET http://localhost:5000/api/EmailTest/configuration-check
```

This will show:
- ✅ API Key prefix (first 10 chars)
- ✅ FromEmail and FromName
- ❌ Any configuration issues

### Step 2: Send Test Email
```bash
POST http://localhost:5000/api/EmailTest/send-test?testEmail=your.actual.email@gmail.com
```

Replace `your.actual.email@gmail.com` with your real email address.

### Step 3: Check Logs
Look at your application console/logs for detailed error messages:
- `UNAUTHORIZED (401)` = Bad API key
- `FORBIDDEN (403)` = Sender email not verified
- `ACCEPTED (202)` = Success!

---

## 📧 Current Configuration

Your current settings from `appsettings.json`:
```json
"SendGridSettings": {
  "ApiKey": "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",
  "FromEmail": "noreply@contracting.app",
  "FromName": "Contracting System"
}
```

### What You Need to Do:

1. ✅ **Verify the API Key is valid** (login to SendGrid and check)
2. ✅ **Verify `noreply@contracting.app`** in SendGrid
3. ✅ **Test** using the `/api/EmailTest/send-test` endpoint

---

## 🔍 Common Issues & Solutions

### Issue: "The from email does not contain a valid address"
- **Solution**: Make sure FromEmail is a valid email format with @domain.com

### Issue: "API key does not start with 'SG.'"
- **Solution**: You're using an old or invalid API key. Generate a new one.

### Issue: "Could not find template file"
- **Solution**: Make sure `wwwroot/emails/reset-password-code.html` exists

### Issue: Email sends but never arrives
- **Solution**: 
  1. Check spam/junk folder
  2. Check SendGrid Activity Feed: https://app.sendgrid.com/email_activity
  3. Verify recipient email is valid

---

## 📊 SendGrid Dashboard

Check your email activity:
- https://app.sendgrid.com/email_activity

This shows all attempted sends, deliveries, bounces, and errors.

---

## 🔐 Security Best Practices

1. **Never commit API keys to Git**
   - Use User Secrets for development: `dotnet user-secrets set "SendGridSettings:ApiKey" "SG.your-key"`
   - Use environment variables in production

2. **Rotate API keys regularly**

3. **Use restricted API keys** in production (only Mail Send permission)

4. **Remove `[AllowAnonymous]`** from EmailTestController in production

---

## Need Help?

1. Check SendGrid logs: https://app.sendgrid.com/email_activity
2. Check application logs for detailed error messages
3. Verify sender: https://app.sendgrid.com/settings/sender_auth
4. SendGrid documentation: https://docs.sendgrid.com/
