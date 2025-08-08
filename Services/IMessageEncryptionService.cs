using Freelancing.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Freelancing.Services
{
    public interface IMessageEncryptionService
    {
        string EncryptMessage(string message, string key);
        string DecryptMessage(string encryptedMessage, string key);
        string GenerateKey();
        string GenerateRoomKey(string mentorshipMatchId);
    }

    // Enhanced entity for storing encrypted messages
    public class EncryptedMentorshipChatMessage : MentorshipChatMessage
    {
        public bool IsEncrypted { get; set; } = true;
        public string? EncryptionMethod { get; set; } = "AES-256";
    }

    public class MessageEncryptionService : IMessageEncryptionService
    {
        private readonly IConfiguration _configuration;

        public MessageEncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string EncryptMessage(string message, string key)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            try
            {
                using (var aes = Aes.Create())
                {
                    // Use the key to derive a 256-bit encryption key
                    var keyBytes = DeriveKey(key, 32); // 32 bytes = 256 bits
                    aes.Key = keyBytes;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Prepend IV to the encrypted data
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't expose details
                Console.WriteLine($"Encryption error: {ex.Message}");
                throw new InvalidOperationException("Failed to encrypt message");
            }
        }

        public string DecryptMessage(string encryptedMessage, string key)
        {
            if (string.IsNullOrEmpty(encryptedMessage))
                return encryptedMessage;

            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedMessage);

                using (var aes = Aes.Create())
                {
                    var keyBytes = DeriveKey(key, 32);
                    aes.Key = keyBytes;

                    // Extract IV from the beginning of the encrypted data
                    var iv = new byte[aes.BlockSize / 8];
                    Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    // Get the actual encrypted content (without IV)
                    var encryptedContent = new byte[encryptedBytes.Length - iv.Length];
                    Array.Copy(encryptedBytes, iv.Length, encryptedContent, 0, encryptedContent.Length);

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(encryptedContent))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
                throw new InvalidOperationException("Failed to decrypt message");
            }
        }

        public string GenerateKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var keyBytes = new byte[32]; // 256 bits
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }

        public string GenerateRoomKey(string mentorshipMatchId)
        {
            // Generate a deterministic but secure key for each mentorship match
            var masterKey = _configuration["Encryption:MasterKey"] ?? throw new InvalidOperationException("Master key not configured");
            var input = $"{masterKey}:{mentorshipMatchId}";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private byte[] DeriveKey(string password, int keySize)
        {
            // Generate a deterministic salt from the password itself
            // This ensures the same password always generates the same key
            var saltString = $"FreelancingAppSalt:{password}:2024";
            var salt = Encoding.UTF8.GetBytes(saltString).Take(16).ToArray(); // Use first 16 bytes

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(keySize);
            }
        }
    }
}