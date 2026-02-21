// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Discounts.Infrastructure.Context;
using Discounts.Web.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly DiscountsDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            DiscountsDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(!ModelState.IsValid) return View(model);
            if (model.Role != "Merchant" && model.Role != "Customer")
            {
                model.Role = "Customer";
            }
            var user = new IdentityUser{UserName = model.Email, Email = model.Email};
            var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role).ConfigureAwait(false);
                if (model.Role == "Merchant")
                {
                    var merchant = new Merchant
                    {
                        Name = model.MerchantName ?? "Unknown Business",
                        Description = model.MerchantDetails ?? "No Description",
                        Number = model.MerchantPhone,
                        Email = model.Email,
                        Status = 0,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Merchants.Add(merchant);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    return RedirectToAction("Index", "Home", new { message = "PendingApproval" });
                }
                await _signInManager.SignInAsync(user, isPersistent:false).ConfigureAwait(false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);

            if (user != null)
            {
                var isMerchant = await _userManager.IsInRoleAsync(user, "Merchant").ConfigureAwait(false);
                if (isMerchant)
                {
                    var merchantProfile = await _context
                        .Merchants
                        .FirstOrDefaultAsync(m => m.Email == user.Email).ConfigureAwait(false);
                    if (merchantProfile != null && merchantProfile.Status == MerchantStatus.Pending)
                    {
                        ModelState.AddModelError(string.Empty, "Dude, wait for pending approval");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }
    }
}
