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
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        public AccountController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUsername = _context.UserAccounts
                    .Any(x => x.UserName.ToLower() == model.UserName.ToLower());

                if (existingUsername)
                {
                    ModelState.AddModelError("UserName", "Username is already taken. Please choose a different username.");
                }

                var existingEmail = _context.UserAccounts
                    .Any(x => x.Email.ToLower() == model.Email.ToLower());

                if (existingEmail)
                {
                    ModelState.AddModelError("Email", "Email is already registered. Please use a different email address.");
                }

                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                UserAccount account = new UserAccount();
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.UserName = model.UserName;
                account.Password = _passwordHasher.HashPassword(account, model.Password);
                account.Role = model.Role;

                try
                {
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

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

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts
                    .Where(x => x.Email == model.UserNameorEmail || x.UserName == model.UserNameorEmail)
                    .FirstOrDefault();

                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    if (result == PasswordVerificationResult.Success ||
                        result == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        if (result == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            user.Password = _passwordHasher.HashPassword(user, model.Password);
                            _context.SaveChanges();
                        }
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

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
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
