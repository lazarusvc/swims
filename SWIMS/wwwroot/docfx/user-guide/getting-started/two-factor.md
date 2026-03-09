# Two-Factor Authentication

Two-factor authentication (2FA) adds a second layer of security to your SWIMS account. After entering your password, you are also asked for a time-based code from an authenticator app on your phone.

> [!NOTE]
> 2FA is only available for **local SWIMS accounts**. If you use your Active Directory (network) username to log in, 2FA is managed by your organisation's IT security policies and is not configured within SWIMS.

## Setting Up 2FA

You will need an authenticator app on your phone. Any standard TOTP app works, including:
- **Google Authenticator** (Android / iOS)
- **Microsoft Authenticator** (Android / iOS)
- **Authy** (Android / iOS / desktop)

### Steps

1. Go to **Account → Two-Factor Authentication**.
2. Click **Set Up Authenticator App**.
3. A **QR code** and a manual setup key are displayed.
4. Open your authenticator app and scan the QR code (or enter the key manually).
5. A 6-digit code will appear in the app. Enter it in the **Verification Code** field on screen.
6. Click **Verify and Enable**.
7. **Save your recovery codes** — these are shown once. Store them somewhere safe (e.g. a printed copy in a secure location).

2FA is now active on your account.

## Logging In With 2FA

1. Enter your username/email and password as normal.
2. On the next screen, open your authenticator app and enter the current **6-digit code**.
3. Codes refresh every 30 seconds. If the code doesn't work, wait for the next one.
4. Click **Verify**.

## Using a Recovery Code

If you do not have access to your authenticator app:

1. On the 2FA verification screen, click **Log in with a recovery code**.
2. Enter one of your saved recovery codes.
3. Each recovery code can only be used **once**.

If you have run out of recovery codes and cannot access your authenticator app, contact your SWIMS administrator to reset your 2FA.

## Disabling 2FA

1. Go to **Account → Two-Factor Authentication**.
2. Click **Disable Two-Factor Authentication**.
3. Confirm the action.

> [!WARNING]
> Disabling 2FA reduces the security of your account. Only do this if instructed by your administrator or if you are replacing it with a new authenticator setup.

## Regenerating Recovery Codes

If you have used most of your recovery codes or they have been compromised:

1. Go to **Account → Two-Factor Authentication**.
2. Click **Generate New Recovery Codes**.
3. Your old codes are immediately invalidated.
4. Save the new codes securely.

## Resetting Your Authenticator App

If you get a new phone or need to move to a different app:

1. Go to **Account → Two-Factor Authentication**.
2. Click **Reset Authenticator App**.
3. This invalidates your current setup.
4. Follow the [Setting Up 2FA](#setting-up-2fa) steps again with your new app.
