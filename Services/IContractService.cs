using Freelancing.Models.Entities;

namespace Freelancing.Services
{
    public interface IContractService
    {
        // Contract Creation & Management
        Task<Contract> CreateContractFromBiddingAsync(Guid projectId, Guid biddingId);
        Task<Contract?> GetContractByProjectIdAsync(Guid projectId);
        Task<Contract?> GetContractByIdAsync(Guid contractId);
        Task<Contract> UpdateContractContentAsync(Guid contractId, string newContent, Guid userId);
        
        // Contract Template Management
        Task<ContractTemplate?> GetContractTemplateAsync(string category);
        Task<string> GenerateContractContentAsync(Project project, Bidding bidding, ContractTemplate template);
        
        // Signature Management
        Task<Contract> SignContractAsync(Guid contractId, Guid userId, string signatureType, string signatureData, string ipAddress, string userAgent);
        Task<bool> IsContractFullySignedAsync(Guid contractId);
        Task<bool> CanUserSignContractAsync(Guid contractId, Guid userId);
        
        // PDF Generation
        Task<byte[]> GenerateContractPdfAsync(Guid contractId);
        Task<string> SaveSignedContractPdfAsync(Guid contractId, byte[] pdfData);
        
        // Audit & Security
        Task LogContractActionAsync(Guid contractId, Guid userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);
        Task<string> CalculateDocumentHashAsync(string content);
        Task<bool> VerifyDocumentIntegrityAsync(Guid contractId);
        
        // Contract Status Management
        Task UpdateContractStatusAsync(Guid contractId, string newStatus, Guid userId);
        Task<List<Contract>> GetContractsByUserIdAsync(Guid userId, string? status = null);
        Task<List<ContractAuditLog>> GetContractAuditLogsAsync(Guid contractId);
        
        // Revision Management
        Task<ContractRevision> CreateContractRevisionAsync(Guid contractId, string newContent, string revisionNotes, Guid userId);
        Task<List<ContractRevision>> GetContractRevisionsAsync(Guid contractId);
    }
}

