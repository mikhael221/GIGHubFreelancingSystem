using Freelancing.Models;
using Freelancing.Models.Entities;

namespace Freelancing.Services
{
    public interface IIdentityVerificationService
    {
        Task<VerificationResultViewModel> VerifyIdentityAsync(IdentityVerificationViewModel model, Guid userId);
        Task<VerificationResultViewModel> CompleteVerificationAsync(string liveFaceImageData, Guid userId, string idDocumentType, string idDocumentNumber, DateTime? idDocumentExpiryDate, bool idDocumentHasNoExpiration, bool idDocumentVerified, float idDocumentConfidence);
        Task<(bool verified, string message, float confidence)> VerifyIdDocumentAsync(IFormFile documentImage, string idDocumentType, string idDocumentNumber, DateTime? idDocumentExpiryDate, bool idDocumentHasNoExpiration, Guid userId);
        Task<(bool verified, string message, float confidence)> VerifyLiveFaceAsync(string base64ImageData, Guid userId);
        Task<VerificationStatusViewModel?> GetVerificationStatusAsync(Guid userId);
        Task<bool> IsUserVerifiedAsync(Guid userId);
        Task<bool> CanUserPostProjectAsync(Guid userId);
        Task<bool> CanUserBidAsync(Guid userId);
        Task<IdentityVerification?> GetLatestVerificationAsync(Guid userId);
        Task<bool> UpdateVerificationStatusAsync(Guid verificationId, string status, string? reason = null);
    }
}
