using Freelancing.Models.Entities;

namespace Freelancing.Services
{
    public interface IPdfGenerationService
    {
        /// <summary>
        /// Generates a PDF document from a contract with signatures embedded
        /// </summary>
        Task<byte[]> GenerateContractPdfAsync(Contract contract);
        
        /// <summary>
        /// Generates a PDF from HTML content
        /// </summary>
        Task<byte[]> GenerateFromHtmlAsync(string htmlContent);
        
        /// <summary>
        /// Adds signature images to an existing PDF
        /// </summary>
        Task<byte[]> AddSignaturesToPdfAsync(byte[] pdfData, string? clientSignature, string? freelancerSignature);
        
        /// <summary>
        /// Validates that a PDF file is not corrupted
        /// </summary>
        Task<bool> ValidatePdfIntegrityAsync(byte[] pdfData);
    }
}

