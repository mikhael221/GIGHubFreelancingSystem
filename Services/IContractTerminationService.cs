using Freelancing.Models.Entities;

namespace Freelancing.Services
{
    public interface IContractTerminationService
    {
        // Termination Request Management
        Task<ContractTermination> CreateTerminationRequestAsync(Guid contractId, Guid userId, string reason, string details, decimal finalPayment, string? settlementNotes);
        Task<ContractTermination?> GetTerminationByIdAsync(Guid terminationId);
        Task<ContractTermination?> GetTerminationByContractIdAsync(Guid contractId);
        Task<List<ContractTermination>> GetTerminationsByUserIdAsync(Guid userId, string? status = null);
        
        // Signature Management
        Task<ContractTermination> SignTerminationAsync(Guid terminationId, Guid userId, string signatureType, string signatureData, string ipAddress, string userAgent);
        Task<bool> IsTerminationFullySignedAsync(Guid terminationId);
        Task<bool> CanUserSignTerminationAsync(Guid terminationId, Guid userId);
        
        // Status Management
        Task UpdateTerminationStatusAsync(Guid terminationId, string newStatus, Guid userId);
        Task CancelTerminationAsync(Guid terminationId, Guid userId);
        Task ExecuteTerminationAsync(Guid terminationId, Guid userId);
        
        // PDF Generation
        Task<byte[]> GenerateTerminationPdfAsync(Guid terminationId);
        Task<string> SaveSignedTerminationPdfAsync(Guid terminationId, byte[] pdfData);
        
        // Audit & Security
        Task LogTerminationActionAsync(Guid terminationId, Guid userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);
        Task<string> CalculateTerminationDocumentHashAsync(string content);
        Task<bool> VerifyTerminationDocumentIntegrityAsync(Guid terminationId);
        
        // Contract Termination
        Task TerminateContractAsync(Guid terminationId, Guid userId);
        Task<List<ContractTerminationAuditLog>> GetTerminationAuditLogsAsync(Guid terminationId);
    }
}
