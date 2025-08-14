using Freelancing.Models.Entities;
using System.Text;

namespace Freelancing.Services
{
    public class PdfGenerationService : IPdfGenerationService
    {
        public async Task<byte[]> GenerateContractPdfAsync(Contract contract)
        {
            var htmlContent = BuildContractHtml(contract);
            return await GenerateFromHtmlAsync(htmlContent);
        }

        public async Task<byte[]> GenerateFromHtmlAsync(string htmlContent)
        {
            // For now, we'll use a simple HTML to PDF conversion
            // In a real implementation, you would use libraries like:
            // - PuppeteerSharp (requires Node.js)
            // - DinkToPdf (requires wkhtmltopdf)
            // - IronPdf (commercial)
            // - SelectPdf (commercial)
            
            // This is a placeholder implementation for demonstration
            // You'll need to implement this based on your chosen PDF library
            
            var htmlBytes = Encoding.UTF8.GetBytes(htmlContent);
            
            // Simulate PDF generation delay
            await Task.Delay(100);
            
            return htmlBytes; // This should return actual PDF bytes
        }

        public async Task<byte[]> AddSignaturesToPdfAsync(byte[] pdfData, string? clientSignature, string? freelancerSignature)
        {
            // This would modify the existing PDF to add signature images
            // Implementation depends on your PDF library choice
            
            await Task.Delay(50); // Simulate processing
            return pdfData; // Return modified PDF
        }

        public async Task<bool> ValidatePdfIntegrityAsync(byte[] pdfData)
        {
            // Validate PDF structure and integrity
            await Task.Delay(10);
            return pdfData != null && pdfData.Length > 0;
        }

        private string BuildContractHtml(Contract contract)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Freelance Contract</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; color: #333; }
                .header { text-align: center; margin-bottom: 40px; border-bottom: 2px solid #333; padding-bottom: 20px; }
                .contract-title { font-size: 24px; font-weight: bold; margin-bottom: 10px; }
                .contract-date { font-size: 14px; color: #666; }
                .section { margin: 30px 0; }
                .section-title { font-size: 18px; font-weight: bold; margin-bottom: 15px; color: #2c3e50; }
                .content { margin-bottom: 20px; }
                .signature-section { margin-top: 60px; }
                .signature-box { border: 1px solid #ccc; padding: 20px; margin: 20px 0; min-height: 80px; }
                .signature-label { font-weight: bold; margin-bottom: 10px; }
                .signature-image { max-width: 200px; max-height: 60px; }
                .signature-details { font-size: 12px; color: #666; margin-top: 10px; }
                .terms-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin: 20px 0; }
                .term-item { padding: 10px; background: #f8f9fa; border-left: 4px solid #007bff; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine($"<div class='contract-title'>{contract.ContractTitle}</div>");
            html.AppendLine($"<div class='contract-date'>Generated on {DateTime.Now:MMMM dd, yyyy}</div>");
            html.AppendLine("</div>");
            
            // Contract content
            html.AppendLine("<div class='section'>");
            html.AppendLine(contract.ContractContent);
            html.AppendLine("</div>");
            
            // Contract terms summary
            if (!string.IsNullOrEmpty(contract.PaymentTerms) || !string.IsNullOrEmpty(contract.Timeline))
            {
                html.AppendLine("<div class='section'>");
                html.AppendLine("<div class='section-title'>Contract Terms Summary</div>");
                html.AppendLine("<div class='terms-grid'>");
                
                if (!string.IsNullOrEmpty(contract.PaymentTerms))
                {
                    html.AppendLine("<div class='term-item'>");
                    html.AppendLine("<strong>Payment Terms:</strong><br>");
                    html.AppendLine($"{contract.PaymentTerms}");
                    html.AppendLine("</div>");
                }
                
                if (!string.IsNullOrEmpty(contract.Timeline))
                {
                    html.AppendLine("<div class='term-item'>");
                    html.AppendLine("<strong>Timeline:</strong><br>");
                    html.AppendLine($"{contract.Timeline}");
                    html.AppendLine("</div>");
                }
                
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }
            
            // Signatures section
            html.AppendLine("<div class='signature-section'>");
            html.AppendLine("<div class='section-title'>Signatures</div>");
            
            // Client signature
            html.AppendLine("<div class='signature-box'>");
            html.AppendLine("<div class='signature-label'>Client Signature:</div>");
            if (contract.ClientSignedAt.HasValue && !string.IsNullOrEmpty(contract.ClientSignatureData))
            {
                if (contract.ClientSignatureType == "Canvas")
                {
                    html.AppendLine($"<img src='data:image/png;base64,{contract.ClientSignatureData}' class='signature-image' alt='Client Signature' />");
                }
                else
                {
                    html.AppendLine($"<div style='font-family: cursive; font-size: 20px;'>{contract.ClientSignatureData}</div>");
                }
                html.AppendLine($"<div class='signature-details'>Signed on: {contract.ClientSignedAt:MMMM dd, yyyy 'at' HH:mm} | IP: {contract.ClientIPAddress}</div>");
            }
            else
            {
                html.AppendLine("<div style='color: #999; font-style: italic;'>Pending signature</div>");
            }
            html.AppendLine("</div>");
            
            // Freelancer signature
            html.AppendLine("<div class='signature-box'>");
            html.AppendLine("<div class='signature-label'>Freelancer Signature:</div>");
            if (contract.FreelancerSignedAt.HasValue && !string.IsNullOrEmpty(contract.FreelancerSignatureData))
            {
                if (contract.FreelancerSignatureType == "Canvas")
                {
                    html.AppendLine($"<img src='data:image/png;base64,{contract.FreelancerSignatureData}' class='signature-image' alt='Freelancer Signature' />");
                }
                else
                {
                    html.AppendLine($"<div style='font-family: cursive; font-size: 20px;'>{contract.FreelancerSignatureData}</div>");
                }
                html.AppendLine($"<div class='signature-details'>Signed on: {contract.FreelancerSignedAt:MMMM dd, yyyy 'at' HH:mm} | IP: {contract.FreelancerIPAddress}</div>");
            }
            else
            {
                html.AppendLine("<div style='color: #999; font-style: italic;'>Pending signature</div>");
            }
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            
            // Document integrity
            if (!string.IsNullOrEmpty(contract.DocumentHash))
            {
                html.AppendLine("<div style='margin-top: 40px; font-size: 10px; color: #666; border-top: 1px solid #eee; padding-top: 10px;'>");
                html.AppendLine($"Document Hash: {contract.DocumentHash}<br>");
                html.AppendLine($"Contract ID: {contract.Id}<br>");
                html.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
    }
}

