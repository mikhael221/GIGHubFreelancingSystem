using System.Security.Claims;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Controllers
{
    // Handles user account management including registration, login, and logout functionalities.
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        // Constructor to initialize the context and password hasher.
        public AccountController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Displays the registration view for new users to create an account.
        public IActionResult Registration()
        {
            return View();
        }
        // Handles the registration of a new user account.
        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username or email already exists in the database.
                var existingUsername = _context.UserAccounts
                    .Any(x => x.UserName.ToLower() == model.UserName.ToLower());

                if (existingUsername)
                {
                    ModelState.AddModelError("UserName", "Username is already taken. Please choose a different username.");
                }

                // Check if the email is already registered.
                var existingEmail = _context.UserAccounts
                    .Any(x => x.Email.ToLower() == model.Email.ToLower());

                if (existingEmail)
                {
                    ModelState.AddModelError("Email", "Email is already registered. Please use a different email address.");
                }

                // Validate that the password and confirm password fields match.
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                }

                // If there are any validation errors, return the view with the model to display the errors.
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Create a new user account with the provided details.
                UserAccount account = new UserAccount();
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.UserName = model.UserName;
                account.Password = _passwordHasher.HashPassword(account, model.Password);
                account.Role = model.Role;

                // Attempt to add the new user account to the database.
                try
                {
                    // Add the new user account to the context and save changes.
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

                    // Clear the ModelState to prevent resubmission and set a success message.
                    ModelState.Clear();
                    ViewBag.Message = "Registration successful!";

                    return View(new RegistrationViewModel());
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                }

                return View();
            }
            return View(model);
        }

        // Displays the login view for existing users to log in to their accounts.
        public IActionResult Login()
        {
            return View();
        }
        // Handles the login process for existing users.
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Validate the login model to ensure all required fields are filled out correctly.
            if (ModelState.IsValid)
            {
                // Check if the user exists in the database by either username or email.
                var user = _context.UserAccounts
                    .Where(x => x.Email == model.UserNameorEmail || x.UserName == model.UserNameorEmail)
                    .FirstOrDefault();

                // If the user is found, verify the password using the password hasher.
                if (user != null)
                {
                    // Verify the hashed password against the provided password from the login model.
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    if (result == PasswordVerificationResult.Success ||
                        result == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        // If the password verification is successful, check if rehashing is needed.
                        if (result == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            user.Password = _passwordHasher.HashPassword(user, model.Password);
                            _context.SaveChanges();
                        }
                        // Create a list of claims for the authenticated user, including their ID, username, email, full name, and role.
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                            new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                            new Claim("Photo", user.Photo ?? string.Empty)
                        };
                        // Create a ClaimsIdentity with the claims and the authentication scheme for cookie-based authentication.
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        // Redirect the user to their respective dashboard based on their role.
                        if (user.Role?.ToLower() == "client")
                        {
                            return RedirectToAction("Dashboard", "Client");
                        }
                        else if (user.Role?.ToLower() == "freelancer")
                        {
                            return RedirectToAction("Dashboard", "Freelancer");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username/email or password.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username/email or password.");
                }
            }
            return View();
        }
        // Logs out the user by clearing the authentication cookie and redirects to the home page.
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
