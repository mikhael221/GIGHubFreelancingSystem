using Freelancing.Data;
using Freelancing.Hubs;
using Freelancing.Models.Entities;
using Freelancing.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Freelancing")));

builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddScoped<IMentorshipMatchingService, MentorshipMatchingService>();

builder.Services.AddScoped<IMessageEncryptionService, MessageEncryptionService>();
builder.Services.AddScoped<IMentorshipSchedulingService, MentorshipSchedulingService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractTerminationService, ContractTerminationService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();

// Smart Hiring Services
builder.Services.AddScoped<ISmartHiringFeatureService, SmartHiringFeatureService>();
builder.Services.AddScoped<ISmartHiringService, SmartHiringService>(); // Back to Scoped due to DbContext dependency
builder.Services.AddSingleton<ILocalRandomForestService, LocalRandomForestService>(); // Local Random Forest
builder.Services.AddHttpClient<SmartHiringService>(); // For Azure ML API calls
builder.Services.AddHttpClient<LocalRandomForestService>(); // For Flask API calls

// Identity Verification Services
builder.Services.AddScoped<IIdentityVerificationService, IdentityVerificationService>();
builder.Services.AddScoped<IIdentityEncryptionService, IdentityEncryptionService>();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Enable for development
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB for file uploads
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Only enable detailed errors in development
    app.UseDeveloperExceptionPage();
}

var masterKey = builder.Configuration["ENCRYPTION_MASTER_KEY"] ??
                builder.Configuration["Encryption:MasterKey"];

if (string.IsNullOrEmpty(masterKey))
{
    throw new InvalidOperationException("Encryption master key not configured");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use Session middleware
app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// Map SignalR Hubs
app.MapHub<MentorshipChatHub>("/mentorshipChatHub");
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await Freelancing.SeedGoals.SeedGoalsData(context);
    await Freelancing.SeedUserSkills.SeedUserSkillsData(context);
    await Freelancing.SeedContractTemplates.SeedAsync(context);
}

// Initialize Random Forest service at startup for faster first use
using (var scope = app.Services.CreateScope())
{
    try
    {
        var randomForestService = scope.ServiceProvider.GetRequiredService<ILocalRandomForestService>();
        
        // Add timeout to prevent app from hanging at startup
        var initTask = randomForestService.EnsureInitializedAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(20)); // 20 second timeout
        
        var completedTask = await Task.WhenAny(initTask, timeoutTask);
        
        if (completedTask == initTask)
        {
            await initTask; // Get any exceptions
            Console.WriteLine("Random Forest service initialized at startup");
        }
        else
        {
            Console.WriteLine("Random Forest initialization timed out - will initialize on first use");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Random Forest initialization failed: {ex.Message}");
        // Don't fail the app startup - the service will initialize on first use
    }
}

// Register cleanup for PDF generation service
app.Lifetime.ApplicationStopping.Register(() =>
{
    var pdfService = app.Services.GetService<IPdfGenerationService>();
    pdfService?.Dispose();
});

app.Run();
