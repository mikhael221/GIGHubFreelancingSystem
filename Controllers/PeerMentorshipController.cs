using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    [Authorize]
    public class PeerMentorshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PeerMentorshipController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult PseudoRegSuccess()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult RegistrationSuccess()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registration(MentorshipRegistration model)
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
                account.UserId = model.UserId;
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
        [HttpGet]
        public async Task<IActionResult> RegistrationSuccess(Guid? Id = null)
        {
            Guid mentorshipId;

            // If no ID is provided, try to get it from the current user's claims
            if (Id == null)
            {
                var mentorshipIdClaim = User.FindFirst("MentorshipId")?.Value;
                if (string.IsNullOrEmpty(mentorshipIdClaim) || !Guid.TryParse(mentorshipIdClaim, out mentorshipId))
                {
                    // Redirect to registration if no mentorship ID found
                    return RedirectToAction("Registration");
                }
            }
            else
            {
                mentorshipId = Id.Value;
            }

            var mentorship = await _context.PeerMentorships
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == mentorshipId);

            if (mentorship == null)
                return NotFound();

            return View(mentorship);
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard(Guid? Id = null)
        {
            Guid mentorshipId;

            // If no ID is provided, try to get it from the current user's claims
            if (Id == null)
            {
                var mentorshipIdClaim = User.FindFirst("MentorshipId")?.Value;
                if (string.IsNullOrEmpty(mentorshipIdClaim) || !Guid.TryParse(mentorshipIdClaim, out mentorshipId))
                {
                    // Redirect to registration if no mentorship ID found
                    return RedirectToAction("Registration");
                }
            }
            else
            {
                mentorshipId = Id.Value;
            }

            var mentorship = await _context.PeerMentorships
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == mentorshipId);

            if (mentorship == null)
                return NotFound();

            return View(mentorship);
        }
    }
}
