# Password Reset with Email Verification - Implementation Summary

## Overview
Implemented a secure forgot/reset password feature using email verification codes instead of reset links.

## Flow
1. **Forgot Password** → User provides email → System sends 6-digit code via email
2. **Verify Code** → User enters code → System validates code and marks as verified
3. **Reset Password** → User provides verified code + new password → System resets password

---

## API Endpoints

### 1. POST `/api/auth/forgot-password`
**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "If the email exists in our system, a verification code has been sent."
}
```

**Features:**
- Generates 6-digit verification code
- Invalidates any existing unused codes for the user
- Code expires in 15 minutes
- Sends email with verification code
- Returns same message regardless of email existence (security best practice)

---

### 2. POST `/api/auth/verify-reset-code`
**Request:**
```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response:**
```json
{
  "message": "Verification code is valid. You can now reset your password."
}
```

**Validations:**
- Code must be 6 digits
- Code must not be used
- Code must not be expired (15 minutes from creation)
- Marks code as verified upon success

---

### 3. POST `/api/auth/reset-password`
**Request:**
```json
{
  "email": "user@example.com",
  "code": "123456",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Response:**
```json
{
  "message": "Password has been reset successfully. You can now login with your new password."
}
```

**Validations:**
- Code must be verified (via verify-reset-code endpoint)
- Code must not be expired (15 minutes from creation)
- Code verification must be recent (within 10 minutes of verification)
- Code must not be used
- Passwords must match
- Password must meet security requirements (min 6 characters)
- Marks code as used after successful password reset

---

## Database Changes

### PasswordResetCode Entity
**Table:** `PasswordResetCodes`
**Schema:** `dbo`

| Column | Type | Description |
|--------|------|-------------|
| Id | Guid | Primary key |
| UserId | Guid | Foreign key to ApplicationUser |
| Code | string | 6-digit verification code |
| ExpiresAt | DateTimeOffset | Expiration timestamp (15 min) |
| IsVerified | bool | Whether code was verified |
| IsUsed | bool | Whether code was used for password reset |
| VerifiedAt | DateTimeOffset? | When code was verified |
| UsedAt | DateTimeOffset? | When code was used |
| CreatedDate | DateTimeOffset | Audit field |
| CreatedBy | Guid | Audit field |
| ModifiedDate | DateTimeOffset? | Audit field |
| ModifiedBy | Guid? | Audit field |
| IsDeleted | bool | Soft delete flag |
| DeletedDate | DateTimeOffset? | Soft delete timestamp |
| DeletedBy | Guid? | Soft delete user |

---

## Files Created/Modified

### New Files:
1. **Domain/Entities/helper/PasswordResetCode.cs** - Entity for storing reset codes
2. **Application/Features/Users/Commands/VerifyResetCodeCommand/** - Verification command files
   - VerifyResetCodeCommand.cs
   - VerifyResetCodeCommandHandler.cs
   - VerifyResetCodeRequestValidator.cs
3. **wwwroot/emails/reset-password-code.html** - Email template for verification code

### Modified Files:
1. **Persistence/ApplicationDbContext.cs** - Added PasswordResetCodes DbSet
2. **Features/Users/Commands/ForgotPasswordCommand/ForgotPasswordCommandHandler.cs**
   - Changed from sending reset link to sending 6-digit code
   - Added code generation and storage
   - Invalidates old unused codes
3. **Features/Users/Commands/ResetPasswordCommand/ResetPasswordCommand.cs**
   - Changed request from Token to Code
4. **Features/Users/Commands/ResetPasswordCommand/ResetPasswordCommandHandler.cs**
   - Added code verification check
   - Added expiration and usage validation
   - Marks code as used after successful reset
5. **Features/Users/Commands/ResetPasswordCommand/ResetPasswordRequestValidator.cs**
   - Updated validation from Token to Code (6-digit requirement)
6. **API/Controllers/AuthController.cs** - Added verify-reset-code endpoint

---

## Security Features

1. **Code Expiration**: All codes expire after 15 minutes
2. **Verification Window**: Verified codes must be used within 10 minutes
3. **Single Use**: Codes can only be used once
4. **Code Invalidation**: Old codes are invalidated when new request is made
5. **Generic Responses**: Doesn't reveal if email exists in system
6. **Soft Delete**: Old codes are soft-deleted, not hard-deleted (audit trail)

---

## Email Template

The system sends an HTML email with:
- Large, prominent 6-digit code display
- Expiration warning (15 minutes)
- Security tips
- Professional styling

---

## Migration

**Migration Name:** `AddPasswordResetCodeTable`
**Applied:** Successfully

To rollback:
```bash
dotnet ef migrations remove --startup-project ..\Contracting.API\Contracting.API.csproj
```

---

## Testing the Flow

### 1. Request Password Reset
```bash
POST /api/auth/forgot-password
{
  "email": "test@example.com"
}
```

### 2. Check Email for Code
User receives email with 6-digit code (e.g., 123456)

### 3. Verify Code
```bash
POST /api/auth/verify-reset-code
{
  "email": "test@example.com",
  "code": "123456"
}
```

### 4. Reset Password
```bash
POST /api/auth/reset-password
{
  "email": "test@example.com",
  "code": "123456",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

---

## Error Handling

The system returns appropriate error messages for:
- Invalid email format
- User not found
- Invalid/expired codes
- Unverified codes
- Password mismatch
- Weak passwords
- Used codes
- Expired verification

All errors use the ErrorOr pattern for consistent error handling.
