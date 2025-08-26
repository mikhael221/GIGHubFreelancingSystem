using System.Security.Cryptography;

namespace Freelancing.Services
{
    public interface IIdentityEncryptionService
    {
        string EncryptIdentityData(string data, string userId);
        string DecryptIdentityData(string encryptedData, string userId);
        string EncryptDocumentImage(byte[] imageBytes, string userId);
        byte[] DecryptDocumentImage(string encryptedImage, string userId);
        string GenerateUserKey(string userId);
        bool IsEncrypted(string data);
    }
}
