using Freelancing.Data;
using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancing
{
    public static class SeedContractTemplates
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if templates already exist
            if (await context.ContractTemplates.AnyAsync())
            {
                return;
            }

            var templates = new List<ContractTemplate>
            {
                new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Standard Freelance Agreement",
                    Description = "A comprehensive freelance contract suitable for most projects",
                    Category = "General",
                    IsActive = true,
                    IsDefault = true,
                    TemplateVersion = "1.0",
                    TemplateContent = GetStandardTemplate()
                },
                new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Web Development Contract",
                    Description = "Specialized contract for web development projects",
                    Category = "Web Development",
                    IsActive = true,
                    IsDefault = false,
                    TemplateVersion = "1.0",
                    TemplateContent = GetWebDevelopmentTemplate()
                },
                new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Mobile App Development Contract",
                    Description = "Contract template for mobile application development",
                    Category = "Mobile Development",
                    IsActive = true,
                    IsDefault = false,
                    TemplateVersion = "1.0",
                    TemplateContent = GetMobileAppTemplate()
                },
                new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Graphic Design Contract",
                    Description = "Contract template for graphic design and creative work",
                    Category = "Design",
                    IsActive = true,
                    IsDefault = false,
                    TemplateVersion = "1.0",
                    TemplateContent = GetGraphicDesignTemplate()
                },
                new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Writing & Content Contract",
                    Description = "Contract template for writing and content creation projects",
                    Category = "Writing",
                    IsActive = true,
                    IsDefault = false,
                    TemplateVersion = "1.0",
                    TemplateContent = GetWritingTemplate()
                }
            };

            context.ContractTemplates.AddRange(templates);
            await context.SaveChangesAsync();
        }

        private static string GetStandardTemplate()
        {
            return @"
                <div class='contract-header'>
                    <h1>FREELANCE SERVICE AGREEMENT</h1>
                    <p><strong>Contract Date:</strong> {{CONTRACT_DATE}}</p>
                </div>

                <div class='parties-section'>
                    <h2>PARTIES</h2>
                    <p>This Freelance Service Agreement (""Agreement"") is entered into between:</p>
                    
                    <div class='party-info'>
                        <h3>Client:</h3>
                        <p><strong>Name:</strong> {{CLIENT_NAME}}</p>
                        <p><strong>Email:</strong> {{CLIENT_EMAIL}}</p>
                    </div>
                    
                    <div class='party-info'>
                        <h3>Freelancer:</h3>
                        <p><strong>Name:</strong> {{FREELANCER_NAME}}</p>
                        <p><strong>Email:</strong> {{FREELANCER_EMAIL}}</p>
                    </div>
                </div>

                <div class='project-section'>
                    <h2>PROJECT DETAILS</h2>
                    <p><strong>Project Name:</strong> {{PROJECT_NAME}}</p>
                    <p><strong>Category:</strong> {{PROJECT_CATEGORY}}</p>
                    <p><strong>Description:</strong></p>
                    <div class='project-description'>{{PROJECT_DESCRIPTION}}</div>
                    
                    <h3>Scope of Work</h3>
                    <div class='proposal-details'>{{PROPOSAL_DETAILS}}</div>
                </div>

                <div class='terms-section'>
                    <h2>TERMS AND CONDITIONS</h2>
                    
                    <h3>1. Payment Terms</h3>
                    <p><strong>Total Project Value:</strong> ${{AGREED_AMOUNT}}</p>
                    <p><strong>Original Budget:</strong> ${{PROJECT_BUDGET}}</p>
                    <p>Payment will be made according to the milestone schedule defined in this contract.</p>
                    
                    <h3>2. Timeline</h3>
                    <p><strong>Delivery Timeline:</strong> {{DELIVERY_TIMELINE}}</p>
                    <p>The Freelancer agrees to complete the project within the specified timeline, subject to timely feedback and approvals from the Client.</p>
                    
                    <h3>3. Deliverables</h3>
                    <p>The Freelancer will deliver the agreed-upon work product as specified in the project description and proposal.</p>
                    
                    <h3>4. Revisions</h3>
                    <p>The contract includes a specified number of revision rounds. Additional revisions may incur extra charges as agreed upon.</p>
                    
                    <h3>5. Intellectual Property</h3>
                    <p>Upon full payment, all intellectual property rights for the delivered work will transfer to the Client, except for any pre-existing intellectual property of the Freelancer.</p>
                    
                    <h3>6. Confidentiality</h3>
                    <p>Both parties agree to maintain confidentiality regarding any proprietary information shared during the course of this project.</p>
                    
                    <h3>7. Termination</h3>
                    <p>Either party may terminate this agreement with written notice. In case of termination, payment will be made for work completed up to the termination date.</p>
                    
                    <h3>8. Dispute Resolution</h3>
                    <p>Any disputes arising from this agreement will be resolved through good faith negotiation. If necessary, disputes will be resolved through binding arbitration.</p>
                </div>

                <div class='agreement-section'>
                    <h2>AGREEMENT</h2>
                    <p>By signing below, both parties agree to the terms and conditions set forth in this agreement.</p>
                    
                    <p><strong>This contract becomes effective upon signatures from both parties.</strong></p>
                </div>
            ";
        }

        private static string GetWebDevelopmentTemplate()
        {
            return @"
                <div class='contract-header'>
                    <h1>WEB DEVELOPMENT SERVICE AGREEMENT</h1>
                    <p><strong>Contract Date:</strong> {{CONTRACT_DATE}}</p>
                </div>

                <div class='parties-section'>
                    <h2>PARTIES</h2>
                    <p>This Web Development Service Agreement (""Agreement"") is entered into between:</p>
                    
                    <div class='party-info'>
                        <h3>Client:</h3>
                        <p><strong>Name:</strong> {{CLIENT_NAME}}</p>
                        <p><strong>Email:</strong> {{CLIENT_EMAIL}}</p>
                    </div>
                    
                    <div class='party-info'>
                        <h3>Web Developer:</h3>
                        <p><strong>Name:</strong> {{FREELANCER_NAME}}</p>
                        <p><strong>Email:</strong> {{FREELANCER_EMAIL}}</p>
                    </div>
                </div>

                <div class='project-section'>
                    <h2>WEB DEVELOPMENT PROJECT</h2>
                    <p><strong>Project Name:</strong> {{PROJECT_NAME}}</p>
                    <p><strong>Description:</strong></p>
                    <div class='project-description'>{{PROJECT_DESCRIPTION}}</div>
                    
                    <h3>Technical Specifications</h3>
                    <div class='proposal-details'>{{PROPOSAL_DETAILS}}</div>
                </div>

                <div class='terms-section'>
                    <h2>WEB DEVELOPMENT TERMS</h2>
                    
                    <h3>1. Payment Terms</h3>
                    <p><strong>Total Development Cost:</strong> ${{AGREED_AMOUNT}}</p>
                    <p>Payment schedule will follow project milestones as defined in this contract.</p>
                    
                    <h3>2. Development Timeline</h3>
                    <p><strong>Estimated Completion:</strong> {{DELIVERY_TIMELINE}}</p>
                    <p>Timeline includes development, testing, and deployment phases.</p>
                    
                    <h3>3. Technical Deliverables</h3>
                    <ul>
                        <li>Fully functional website/web application</li>
                        <li>Source code and documentation</li>
                        <li>Testing and quality assurance</li>
                        <li>Deployment and launch support</li>
                    </ul>
                    
                    <h3>4. Browser Compatibility</h3>
                    <p>The website will be tested and compatible with major modern browsers (Chrome, Firefox, Safari, Edge).</p>
                    
                    <h3>5. Responsive Design</h3>
                    <p>The website will be responsive and optimized for desktop, tablet, and mobile devices.</p>
                    
                    <h3>6. Content Management</h3>
                    <p>If applicable, the Developer will provide training for content management system usage.</p>
                    
                    <h3>7. Hosting and Domain</h3>
                    <p>Client is responsible for hosting and domain registration unless otherwise specified.</p>
                    
                    <h3>8. Maintenance Period</h3>
                    <p>A 30-day warranty period is included for bug fixes and minor adjustments after project completion.</p>
                    
                    <h3>9. Code Ownership</h3>
                    <p>Upon full payment, the Client will own all custom code developed for this project.</p>
                </div>

                <div class='agreement-section'>
                    <h2>AGREEMENT</h2>
                    <p>By signing below, both parties agree to the terms and conditions set forth in this web development agreement.</p>
                </div>
            ";
        }

        private static string GetMobileAppTemplate()
        {
            return @"
                <div class='contract-header'>
                    <h1>MOBILE APP DEVELOPMENT AGREEMENT</h1>
                    <p><strong>Contract Date:</strong> {{CONTRACT_DATE}}</p>
                </div>

                <div class='parties-section'>
                    <h2>PARTIES</h2>
                    <div class='party-info'>
                        <h3>Client:</h3>
                        <p><strong>Name:</strong> {{CLIENT_NAME}}</p>
                        <p><strong>Email:</strong> {{CLIENT_EMAIL}}</p>
                    </div>
                    
                    <div class='party-info'>
                        <h3>Mobile Developer:</h3>
                        <p><strong>Name:</strong> {{FREELANCER_NAME}}</p>
                        <p><strong>Email:</strong> {{FREELANCER_EMAIL}}</p>
                    </div>
                </div>

                <div class='project-section'>
                    <h2>MOBILE APPLICATION PROJECT</h2>
                    <p><strong>App Name:</strong> {{PROJECT_NAME}}</p>
                    <p><strong>Description:</strong></p>
                    <div class='project-description'>{{PROJECT_DESCRIPTION}}</div>
                    
                    <h3>Technical Requirements</h3>
                    <div class='proposal-details'>{{PROPOSAL_DETAILS}}</div>
                </div>

                <div class='terms-section'>
                    <h2>MOBILE DEVELOPMENT TERMS</h2>
                    
                    <h3>1. Development Cost</h3>
                    <p><strong>Total Project Cost:</strong> ${{AGREED_AMOUNT}}</p>
                    
                    <h3>2. Development Timeline</h3>
                    <p><strong>Estimated Completion:</strong> {{DELIVERY_TIMELINE}}</p>
                    
                    <h3>3. Platform Specifications</h3>
                    <p>The application will be developed for the agreed platforms (iOS, Android, or both).</p>
                    
                    <h3>4. App Store Submission</h3>
                    <p>Developer will assist with app store submission process and provide necessary assets.</p>
                    
                    <h3>5. Testing and Quality Assurance</h3>
                    <p>Comprehensive testing will be performed on target devices and operating system versions.</p>
                    
                    <h3>6. Source Code and Documentation</h3>
                    <p>Complete source code and technical documentation will be provided upon project completion.</p>
                    
                    <h3>7. Post-Launch Support</h3>
                    <p>30-day post-launch support is included for critical bug fixes.</p>
                    
                    <h3>8. App Store Guidelines</h3>
                    <p>The app will be developed in compliance with respective app store guidelines.</p>
                </div>

                <div class='agreement-section'>
                    <h2>AGREEMENT</h2>
                    <p>By signing below, both parties agree to the terms and conditions set forth in this mobile app development agreement.</p>
                </div>
            ";
        }

        private static string GetGraphicDesignTemplate()
        {
            return @"
                <div class='contract-header'>
                    <h1>GRAPHIC DESIGN SERVICE AGREEMENT</h1>
                    <p><strong>Contract Date:</strong> {{CONTRACT_DATE}}</p>
                </div>

                <div class='parties-section'>
                    <h2>PARTIES</h2>
                    <div class='party-info'>
                        <h3>Client:</h3>
                        <p><strong>Name:</strong> {{CLIENT_NAME}}</p>
                        <p><strong>Email:</strong> {{CLIENT_EMAIL}}</p>
                    </div>
                    
                    <div class='party-info'>
                        <h3>Designer:</h3>
                        <p><strong>Name:</strong> {{FREELANCER_NAME}}</p>
                        <p><strong>Email:</strong> {{FREELANCER_EMAIL}}</p>
                    </div>
                </div>

                <div class='project-section'>
                    <h2>DESIGN PROJECT</h2>
                    <p><strong>Project Name:</strong> {{PROJECT_NAME}}</p>
                    <p><strong>Description:</strong></p>
                    <div class='project-description'>{{PROJECT_DESCRIPTION}}</div>
                    
                    <h3>Design Requirements</h3>
                    <div class='proposal-details'>{{PROPOSAL_DETAILS}}</div>
                </div>

                <div class='terms-section'>
                    <h2>DESIGN TERMS</h2>
                    
                    <h3>1. Design Fee</h3>
                    <p><strong>Total Design Fee:</strong> ${{AGREED_AMOUNT}}</p>
                    
                    <h3>2. Project Timeline</h3>
                    <p><strong>Estimated Completion:</strong> {{DELIVERY_TIMELINE}}</p>
                    
                    <h3>3. Design Deliverables</h3>
                    <p>Final deliverables will include high-resolution files in agreed formats (AI, PSD, PNG, JPG, PDF, etc.).</p>
                    
                    <h3>4. Revision Process</h3>
                    <p>The project includes a specified number of revision rounds. Each revision cycle includes feedback and refinements.</p>
                    
                    <h3>5. Copyright and Usage Rights</h3>
                    <p>Upon full payment, the Client receives full rights to use the final designs for their intended purpose.</p>
                    
                    <h3>6. Stock Images and Fonts</h3>
                    <p>Client is responsible for licensing any stock images or premium fonts required for the project.</p>
                    
                    <h3>7. Brand Guidelines</h3>
                    <p>If applicable, the Designer will provide brand guidelines and usage instructions.</p>
                    
                    <h3>8. Print Specifications</h3>
                    <p>For print projects, final files will be provided in print-ready format with appropriate color profiles.</p>
                </div>

                <div class='agreement-section'>
                    <h2>AGREEMENT</h2>
                    <p>By signing below, both parties agree to the terms and conditions set forth in this graphic design agreement.</p>
                </div>
            ";
        }

        private static string GetWritingTemplate()
        {
            return @"
                <div class='contract-header'>
                    <h1>WRITING & CONTENT CREATION AGREEMENT</h1>
                    <p><strong>Contract Date:</strong> {{CONTRACT_DATE}}</p>
                </div>

                <div class='parties-section'>
                    <h2>PARTIES</h2>
                    <div class='party-info'>
                        <h3>Client:</h3>
                        <p><strong>Name:</strong> {{CLIENT_NAME}}</p>
                        <p><strong>Email:</strong> {{CLIENT_EMAIL}}</p>
                    </div>
                    
                    <div class='party-info'>
                        <h3>Writer:</h3>
                        <p><strong>Name:</strong> {{FREELANCER_NAME}}</p>
                        <p><strong>Email:</strong> {{FREELANCER_EMAIL}}</p>
                    </div>
                </div>

                <div class='project-section'>
                    <h2>WRITING PROJECT</h2>
                    <p><strong>Project Name:</strong> {{PROJECT_NAME}}</p>
                    <p><strong>Description:</strong></p>
                    <div class='project-description'>{{PROJECT_DESCRIPTION}}</div>
                    
                    <h3>Content Requirements</h3>
                    <div class='proposal-details'>{{PROPOSAL_DETAILS}}</div>
                </div>

                <div class='terms-section'>
                    <h2>WRITING TERMS</h2>
                    
                    <h3>1. Writing Fee</h3>
                    <p><strong>Total Project Fee:</strong> ${{AGREED_AMOUNT}}</p>
                    
                    <h3>2. Delivery Timeline</h3>
                    <p><strong>Estimated Completion:</strong> {{DELIVERY_TIMELINE}}</p>
                    
                    <h3>3. Content Deliverables</h3>
                    <p>All content will be delivered in the requested format (Word, Google Docs, HTML, etc.).</p>
                    
                    <h3>4. Research and Fact-Checking</h3>
                    <p>The Writer will conduct necessary research and ensure accuracy of factual content.</p>
                    
                    <h3>5. Originality and Plagiarism</h3>
                    <p>All content will be original and plagiarism-free. The Writer guarantees no copyright infringement.</p>
                    
                    <h3>6. SEO Optimization</h3>
                    <p>If applicable, content will be optimized for search engines according to provided guidelines.</p>
                    
                    <h3>7. Revision Policy</h3>
                    <p>The project includes specified revision rounds for content refinement and feedback incorporation.</p>
                    
                    <h3>8. Usage Rights</h3>
                    <p>Upon payment, the Client receives full rights to use, modify, and publish the content.</p>
                    
                    <h3>9. Attribution</h3>
                    <p>Writer attribution requirements, if any, will be specified in the project details.</p>
                </div>

                <div class='agreement-section'>
                    <h2>AGREEMENT</h2>
                    <p>By signing below, both parties agree to the terms and conditions set forth in this writing agreement.</p>
                </div>
            ";
        }
    }
}

