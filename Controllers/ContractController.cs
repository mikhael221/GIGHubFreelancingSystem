using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Freelancing.Services;
using System.Security.Claims;
using System.Text.Json;

namespace Freelancing.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractService _contractService;
        private readonly INotificationService _notificationService;

        public ContractController(ApplicationDbContext context, IContractService contractService, INotificationService notificationService)
        {
            _context = context;
            _contractService = contractService;
            _notificationService = notificationService;
        }



        // GET: Contract/Create/{projectId}
        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.AcceptedBid)
                    .ThenInclude(ab => ab.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.AcceptedBidId == null)
                return NotFound("Project or accepted bid not found");

            var userId = GetCurrentUserId();
            if (project.UserId != userId)
                return Forbid("You are not authorized to create a contract for this project");
                
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Project Name: {project.ProjectName}");
            System.Diagnostics.Debug.WriteLine($"Project Budget: {project.Budget}");
            System.Diagnostics.Debug.WriteLine($"AcceptedBid Budget: {project.AcceptedBid?.Budget}");

            // Check if contract already exists
            var existingContract = await _contractService.GetContractByProjectIdAsync(projectId);
            if (existingContract != null)
                return RedirectToAction("Details", new { id = existingContract.Id });

            var availableTemplates = await GetAvailableTemplatesAsync(project.Category);
            
            // Ensure we have a default selection if no template is marked as default
            if (availableTemplates.Any() && !availableTemplates.Any(t => t.IsDefault))
            {
                availableTemplates.First().IsDefault = true;
            }

            var viewModel = new CreateContractViewModel
            {
                ProjectId = projectId,
                BiddingId = project.AcceptedBidId.Value,
                ProjectName = project.ProjectName ?? "",
                ProjectDescription = project.ProjectDescription ?? "",
                ProjectBudget = project.Budget ?? "0", // Budget is already a string
                ProjectCategory = project.Category ?? "",
                ClientName = $"{project.User?.FirstName ?? ""} {project.User?.LastName ?? ""}".Trim(),
                ClientEmail = project.User?.Email ?? "",
                FreelancerName = $"{project.AcceptedBid?.User?.FirstName ?? ""} {project.AcceptedBid?.User?.LastName ?? ""}".Trim(),
                FreelancerEmail = project.AcceptedBid?.User?.Email ?? "",
                AgreedAmount = project.AcceptedBid?.Budget ?? 0, // Budget is int
                DeliveryTimeline = project.AcceptedBid?.Delivery ?? "",
                Proposal = project.AcceptedBid?.Proposal ?? "",
                AvailableTemplates = availableTemplates
            };

            return View(viewModel);
        }

        // POST: Contract/Create
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(CreateContractViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTemplates = await GetAvailableTemplatesAsync(model.ProjectCategory);
                return View(model);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating contract for Project: {model.ProjectId}, Bidding: {model.BiddingId}");
                
                var contract = await _contractService.CreateContractFromBiddingAsync(model.ProjectId, model.BiddingId);
                System.Diagnostics.Debug.WriteLine($"Contract created with ID: {contract.Id}");
                
                // Update contract with custom terms
                await UpdateContractTermsAsync(contract.Id, model);
                System.Diagnostics.Debug.WriteLine("Contract terms updated");

                // Send notification to freelancer
                var freelancerId = await GetFreelancerIdFromProjectAsync(model.ProjectId);
                if (freelancerId != Guid.Empty)
                {
                    await _notificationService.CreateNotificationAsync(
                        freelancerId,
                        "Contract Created",
                        $"A contract has been created for project '{model.ProjectName}'. Please review and sign.",
                        "contract_created",
                        null,
                        $"/Contract/Sign/{contract.Id}"
                    );
                    System.Diagnostics.Debug.WriteLine($"Notification sent to freelancer: {freelancerId}");
                }

                TempData["Message"] = "Contract created successfully! Please review and sign to begin the project. The freelancer has been notified.";
                return RedirectToAction("Details", "ManageProjectClient", new { id = model.ProjectId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating contract: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"Error creating contract: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"Inner exception: {ex.InnerException.Message}");
                }
                
                model.AvailableTemplates = await GetAvailableTemplatesAsync(model.ProjectCategory);
                return View(model);
            }
        }

        // GET: Contract/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (!CanUserAccessContract(contract, userId))
                return Forbid();

            var viewModel = MapToContractViewModel(contract);
            
            // Log contract view
            await _contractService.LogContractActionAsync(id, userId, "Viewed", "Contract details viewed", GetClientIpAddress(), Request.Headers["User-Agent"]);

            return View(viewModel);
        }

        // GET: Contract/Sign/{id}
        [HttpGet]
        public async Task<IActionResult> Sign(Guid id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (!CanUserAccessContract(contract, userId))
                return Forbid();

            if (!await _contractService.CanUserSignContractAsync(id, userId))
            {
                TempData["ErrorMessage"] = "You cannot sign this contract at this time.";
                return RedirectToAction("Details", new { id });
            }

            var isClient = contract.Project.UserId == userId;
            var isFreelancer = contract.Bidding.UserId == userId;

            var viewModel = new ContractSigningViewModel
            {
                ContractId = id,
                ContractTitle = contract.ContractTitle,
                ContractContent = contract.ContractContent,
                Status = contract.Status,
                ProjectName = contract.Project.ProjectName,
                ClientName = $"{contract.Project.User.FirstName} {contract.Project.User.LastName}",
                FreelancerName = $"{contract.Bidding.User.FirstName} {contract.Bidding.User.LastName}",
                AgreedAmount = contract.Bidding.Budget,
                DeliveryTimeline = contract.Bidding.Delivery,
                CurrentUserId = userId,
                IsClient = isClient,
                IsFreelancer = isFreelancer,
                CanSign = true
            };

            if (isClient)
            {
                viewModel.HasAlreadySigned = contract.ClientSignedAt.HasValue;
                viewModel.OtherPartyHasSigned = contract.FreelancerSignedAt.HasValue;
                viewModel.OtherPartySignedAt = contract.FreelancerSignedAt?.ToString("MMMM dd, yyyy");
            }
            else
            {
                viewModel.HasAlreadySigned = contract.FreelancerSignedAt.HasValue;
                viewModel.OtherPartyHasSigned = contract.ClientSignedAt.HasValue;
                viewModel.OtherPartySignedAt = contract.ClientSignedAt?.ToString("MMMM dd, yyyy");
            }

            return View(viewModel);
        }

        // POST: Contract/Sign
        [HttpPost]
        public async Task<IActionResult> Sign(ContractSigningViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"];

                var contract = await _contractService.SignContractAsync(
                    model.ContractId, 
                    userId, 
                    model.SignatureType!, 
                    model.SignatureData!, 
                    ipAddress, 
                    userAgent!
                );

                // Send notifications
                if (await _contractService.IsContractFullySignedAsync(model.ContractId))
                {
                    // Contract is fully signed - notify both parties
                    await NotifyContractFullySignedAsync(contract);
                    TempData["SuccessMessage"] = "Contract signed successfully! The project is now active.";
                }
                else
                {
                    // Partial signature - notify the other party
                    await NotifyPartialSignatureAsync(contract, userId);
                    TempData["SuccessMessage"] = "Contract signed successfully! Waiting for the other party to sign.";
                }

                return RedirectToAction("Details", new { id = model.ContractId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error signing contract: {ex.Message}");
                return View(model);
            }
        }

        // GET: Contract/Download/{id}
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (!CanUserAccessContract(contract, userId))
                return Forbid();

            try
            {
                var pdfData = await _contractService.GenerateContractPdfAsync(id);
                
                // Log download
                await _contractService.LogContractActionAsync(id, userId, "Downloaded", "Contract PDF downloaded", GetClientIpAddress(), Request.Headers["User-Agent"]);

                var fileName = $"Contract_{contract.Project.ProjectName}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Contract/MyContracts
        [HttpGet]
        public async Task<IActionResult> MyContracts(string? status = null)
        {
            var userId = GetCurrentUserId();
            var contracts = await _contractService.GetContractsByUserIdAsync(userId, status);
            
            var viewModels = new List<ContractViewModel>();
            foreach (var contract in contracts)
            {
                viewModels.Add(MapToContractViewModel(contract));
            }

            ViewBag.CurrentStatus = status;
            ViewBag.StatusOptions = new[]
            {
                new { Value = "", Text = "All Contracts" },
                new { Value = "Draft", Text = "Draft" },
                new { Value = "AwaitingFreelancer", Text = "Awaiting Freelancer" },
                new { Value = "Active", Text = "Active" },
                new { Value = "Completed", Text = "Completed" }
            };

            return View(viewModels);
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private bool CanUserAccessContract(Contract contract, Guid userId)
        {
            return contract.Project.UserId == userId || contract.Bidding.UserId == userId;
        }

        private async Task<List<ContractTemplateOption>> GetAvailableTemplatesAsync(string? category)
        {
            var templates = await _context.ContractTemplates
                .Where(ct => ct.IsActive)
                .Select(ct => new ContractTemplateOption
                {
                    Id = ct.Id,
                    Name = ct.Name,
                    Description = ct.Description,
                    Category = ct.Category,
                    IsDefault = ct.IsDefault,
                    PreviewImagePath = ct.PreviewImagePath
                })
                .ToListAsync();

            // Order templates: exact category match first, then default, then others
            return templates
                .OrderBy(ct => !string.IsNullOrEmpty(category) && 
                              ct.Category?.ToLower() == category.ToLower() ? 0 : 
                              ct.IsDefault ? 1 : 2)
                .ThenBy(ct => ct.Name)
                .ToList();
        }

        private async Task<Guid> GetFreelancerIdFromProjectAsync(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.AcceptedBid)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            return project?.AcceptedBid?.UserId ?? Guid.Empty;
        }

        private async Task UpdateContractTermsAsync(Guid contractId, CreateContractViewModel model)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return;

            // Update payment terms
            var paymentTerms = new
            {
                upfront = model.UpfrontPercentage,
                final = model.FinalPercentage,
                milestones = model.Milestones.Select(m => new
                {
                    name = m.Name,
                    description = m.Description,
                    percentage = m.Percentage,
                    dueDate = m.DueDate
                }).ToList()
            };
            contract.PaymentTerms = JsonSerializer.Serialize(paymentTerms);

            // Update revision policy
            var revisionPolicy = new
            {
                freeRevisions = model.FreeRevisions,
                additionalCost = model.AdditionalRevisionCost,
                scope = model.RevisionScope
            };
            contract.RevisionPolicy = JsonSerializer.Serialize(revisionPolicy);

            // Update timeline
            var timeline = new
            {
                startDate = model.StartDate,
                deadline = model.Deadline,
                milestones = model.Milestones.Where(m => m.DueDate.HasValue).Select(m => new
                {
                    name = m.Name,
                    dueDate = m.DueDate
                }).ToList()
            };
            contract.Timeline = JsonSerializer.Serialize(timeline);

            // Update deliverable requirements
            contract.DeliverableRequirements = JsonSerializer.Serialize(model.DeliverableRequirements);

            await _context.SaveChangesAsync();
        }

        private ContractViewModel MapToContractViewModel(Contract contract)
        {
            var viewModel = new ContractViewModel
            {
                Id = contract.Id,
                ContractTitle = contract.ContractTitle,
                ContractContent = contract.ContractContent,
                Status = contract.Status,
                CreatedAt = contract.CreatedAt,
                LastModifiedAt = contract.LastModifiedAt,
                ProjectId = contract.ProjectId,
                ProjectName = contract.Project.ProjectName,
                ProjectDescription = contract.Project.ProjectDescription,
                ProjectBudget = contract.Project.Budget,
                ProjectCategory = contract.Project.Category,
                ClientId = contract.Project.UserId,
                ClientName = $"{contract.Project.User.FirstName} {contract.Project.User.LastName}",
                ClientEmail = contract.Project.User.Email,
                ClientPhoto = contract.Project.User.Photo,
                ClientSignedAt = contract.ClientSignedAt,
                ClientSignatureData = contract.ClientSignatureData,
                ClientSignatureType = contract.ClientSignatureType,
                FreelancerId = contract.Bidding.UserId,
                FreelancerName = $"{contract.Bidding.User.FirstName} {contract.Bidding.User.LastName}",
                FreelancerEmail = contract.Bidding.User.Email,
                FreelancerPhoto = contract.Bidding.User.Photo,
                FreelancerSignedAt = contract.FreelancerSignedAt,
                FreelancerSignatureData = contract.FreelancerSignatureData,
                FreelancerSignatureType = contract.FreelancerSignatureType,
                AgreedAmount = contract.Bidding.Budget,
                DeliveryTimeline = contract.Bidding.Delivery,
                Proposal = contract.Bidding.Proposal,
                DocumentPath = contract.DocumentPath
            };

            // Parse JSON fields
            if (!string.IsNullOrEmpty(contract.PaymentTerms))
            {
                try
                {
                    var paymentTermsJson = JsonSerializer.Deserialize<JsonElement>(contract.PaymentTerms);
                    viewModel.PaymentTerms = new PaymentTermsViewModel
                    {
                        UpfrontPercentage = paymentTermsJson.GetProperty("upfront").GetInt32(),
                        FinalPercentage = paymentTermsJson.GetProperty("final").GetInt32()
                    };
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(contract.DeliverableRequirements))
            {
                try
                {
                    viewModel.DeliverableRequirements = JsonSerializer.Deserialize<List<string>>(contract.DeliverableRequirements);
                }
                catch { }
            }

            // Map audit logs
            viewModel.AuditLogs = contract.AuditLogs?.Select(al => new ContractAuditLogViewModel
            {
                Id = al.Id,
                UserName = $"{al.User.FirstName} {al.User.LastName}",
                Action = al.Action,
                Details = al.Details,
                Timestamp = al.Timestamp,
                IPAddress = al.IPAddress
            }).ToList() ?? new List<ContractAuditLogViewModel>();

            return viewModel;
        }

        private async Task NotifyContractFullySignedAsync(Contract contract)
        {
            var clientMessage = $"Contract for '{contract.Project.ProjectName}' has been fully signed. The project is now active!";
            var freelancerMessage = $"Contract for '{contract.Project.ProjectName}' has been fully signed. You can now start working on the project!";

            await _notificationService.CreateNotificationAsync(
                contract.Project.UserId,
                "Contract Fully Signed",
                clientMessage,
                "Contract",
                null,
                $"/Contract/Details/{contract.Id}"
            );

            await _notificationService.CreateNotificationAsync(
                contract.Bidding.UserId,
                "Contract Fully Signed",
                freelancerMessage,
                "Contract",
                null,
                $"/Contract/Details/{contract.Id}"
            );
        }

        private async Task NotifyPartialSignatureAsync(Contract contract, Guid signerUserId)
        {
            var isClientSigner = contract.Project.UserId == signerUserId;
            var recipientId = isClientSigner ? contract.Bidding.UserId : contract.Project.UserId;
            var signerName = isClientSigner ? 
                $"{contract.Project.User.FirstName} {contract.Project.User.LastName}" : 
                $"{contract.Bidding.User.FirstName} {contract.Bidding.User.LastName}";

            var message = $"{signerName} has signed the contract for '{contract.Project.ProjectName}'. Please review and sign to activate the project.";

            await _notificationService.CreateNotificationAsync(
                recipientId,
                "Contract Signature Required",
                message,
                "Contract",
                null,
                $"/Contract/Sign/{contract.Id}"
            );
        }

        #endregion
    }
}
