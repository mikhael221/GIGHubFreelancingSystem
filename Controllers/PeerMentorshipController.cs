using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    public class PeerMentorshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PeerMentorshipController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }
        public IActionResult Role()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public async Task <IActionResult> Registration(MentorshipRegistration model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already registered.
                var existingEmail = _context.PeerMentorships
                    .Any(x => x.Email.ToLower() == model.Email.ToLower());

                if (existingEmail)
                {
                    ModelState.AddModelError("Email", "Email is already registered. Please use a different email address.");
                }

                // If there are any validation errors, return the view with the model to display the errors.
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Create a new user account with the provided details.
                PeerMentorship account = new PeerMentorship();
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.Role = model.Role;

                var existingUser = _context.UserAccounts
                .FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

                // Attempt to add the new user account to the database.
                try
                {
                    // Add the new user account to the context and save changes.
                    _context.PeerMentorships.Add(account);
                    _context.SaveChanges();
                    if (existingUser != null)
                    {
                        existingUser.MentorshipId = account.Id;
                        existingUser.Mentorship = account;
                        _context.UserAccounts.Update(existingUser);
                    }
                    await _context.SaveChangesAsync();
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                        new Claim(ClaimTypes.Name, existingUser.UserName),
                        new Claim(ClaimTypes.Email, existingUser.Email),
                        new Claim(ClaimTypes.GivenName, existingUser.FirstName),
                        new Claim(ClaimTypes.Surname, existingUser.LastName),
                        new Claim("FullName", $"{existingUser.FirstName} {existingUser.LastName}"),
                        new Claim(ClaimTypes.Role, existingUser.Role ?? string.Empty),
                        new Claim("Photo", existingUser.Photo ?? string.Empty),
                        new Claim("MentorshipId", account.Id.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    // Sign in the user again with updated claims
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    // Redirect to success page instead of clearing ModelState
                    TempData["SuccessMessage"] = "Registration successful!";
                    TempData["UserName"] = $"{account.FirstName} {account.LastName}";
                    TempData["UserRole"] = account.Role;

                    return RedirectToAction("RegistrationSuccess");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                    return View(model);
                }
            }

            return View(model);
        }
        public IActionResult RegistrationSuccess()
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Registration");
            }

            return View();
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
