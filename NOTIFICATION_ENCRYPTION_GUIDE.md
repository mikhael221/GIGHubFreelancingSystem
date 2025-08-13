# Notification Encryption Guide

## Overview

The notification system now supports **AES-256 encryption** for sensitive notification content. This feature allows you to encrypt notification titles and messages before storing them in the database, providing an additional layer of security for sensitive information.

## Features

- **AES-256 Encryption**: Industry-standard encryption algorithm
- **User-specific Keys**: Each user has their own encryption key derived from their UserId
- **Backward Compatibility**: Existing notifications continue to work without changes
- **Automatic Decryption**: Notifications are automatically decrypted when retrieved
- **Real-time Support**: Encrypted notifications work with SignalR real-time updates

## How It Works

### Encryption Process
1. When creating a notification with `encryptContent: true`:
   - The system generates a unique encryption key for the user
   - Both title and message are encrypted using AES-256
   - Encrypted content is stored in `EncryptedTitle` and `EncryptedMessage` fields
   - Plain text fields are set to `[ENCRYPTED]` for security
   - `IsEncrypted` flag is set to `true`

### Decryption Process
1. When retrieving notifications:
   - The system checks the `IsEncrypted` flag
   - If encrypted, it uses the user's key to decrypt the content
   - Decrypted content is returned in the normal `Title` and `Message` fields
   - If decryption fails, the original encrypted notification is returned

## Usage Examples

### Creating Encrypted Notifications

```csharp
// In your controller or service
var notification = await _notificationService.CreateNotificationAsync(
    userId: userId,
    title: "Sensitive Information",
    message: "This contains confidential data that should be encrypted",
    type: "sensitive",
    encryptContent: true // Enable encryption
);
```

### Creating Regular Notifications

```csharp
// Regular notification (not encrypted)
var notification = await _notificationService.CreateNotificationAsync(
    userId: userId,
    title: "General Update",
    message: "This is a regular notification",
    type: "general",
    encryptContent: false // Keep unencrypted (default)
);
```

### API Endpoints

The `NotificationController` now includes example endpoints:

- `POST /Notification/CreateEncryptedNotification` - Create encrypted notification
- `POST /Notification/CreateRegularNotification` - Create regular notification

## Database Schema

The `Notifications` table now includes these additional fields:

```sql
-- New encryption fields
IsEncrypted bit NOT NULL DEFAULT 0
EncryptionMethod nvarchar(max) NULL
EncryptedTitle nvarchar(max) NULL
EncryptedMessage nvarchar(max) NULL
```

## Security Considerations

### Key Management
- Encryption keys are derived from the user's UserId and a master key
- Keys are deterministic (same user always gets the same key)
- Master key is stored in configuration (`appsettings.json`)

### Data Protection
- Encrypted content is stored separately from plain text
- Plain text fields show `[ENCRYPTED]` when content is encrypted
- Decryption only happens when the user is authenticated

### Best Practices
1. **Use encryption for sensitive data**: Personal information, financial data, confidential messages
2. **Keep regular notifications unencrypted**: General updates, system notifications
3. **Monitor encryption usage**: Track which notifications are encrypted for audit purposes
4. **Secure master key**: Ensure the encryption master key is properly secured

## Migration Notes

- Existing notifications remain unencrypted
- New encryption fields are optional and nullable
- The system automatically handles both encrypted and unencrypted notifications
- No data migration is required for existing notifications

## Configuration

Ensure your `appsettings.json` includes the encryption configuration:

```json
{
  "Encryption": {
    "MasterKey": "#{ENCRYPTION_MASTER_KEY}#",
    "KeyDerivationIterations": 100000
  }
}
```

## Error Handling

The system includes robust error handling:
- If decryption fails, the original encrypted notification is returned
- Encryption errors are logged but don't expose sensitive information
- Graceful fallback ensures the system continues to function

## Performance Considerations

- Encryption/decryption adds minimal overhead
- Keys are cached per user session
- Bulk operations are optimized to minimize encryption operations

## Testing

To test the encryption feature:

1. Create an encrypted notification using the API
2. Verify the database shows encrypted content
3. Retrieve the notification and verify it's properly decrypted
4. Test with real-time updates via SignalR

## Support

For questions or issues with notification encryption:
1. Check the application logs for encryption-related errors
2. Verify the encryption configuration is correct
3. Ensure the database migration has been applied
4. Test with both encrypted and unencrypted notifications
