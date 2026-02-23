using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Discounts.Web.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IMerchantService _merchantService;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IMerchantService merchantService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _merchantService = merchantService;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if(!ModelState.IsValid) return View(model);
        if (model.Role != "Merchant" && model.Role != "Customer") model.Role = "Customer";

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role).ConfigureAwait(false);
            if (model.Role == "Merchant")
            {
                await _merchantService.CreateMerchantProfileAsync(new Merchant
                {
                    Name = model.MerchantName ?? "Unknown Business",
                    Description = model.MerchantDetails ?? "No Description",
                    Number = model.MerchantPhone,
                    Email = model.Email
                }).ConfigureAwait(false);
                return RedirectToAction("Index", "Home", new { message = "PendingApproval" });
            }
            await _signInManager.SignInAsync(user, isPersistent:false).ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);

        if (user != null)
        {
            if (await _userManager.IsInRoleAsync(user, "Merchant").ConfigureAwait(false) &&
                await _merchantService.IsMerchantPendingAsync(user.Email).ConfigureAwait(false))
            {
                ModelState.AddModelError(string.Empty, "Dude, wait for pending approval");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                if (roles.Contains("Admin")) return RedirectToAction("Index", "Home");
                if (roles.Contains("Merchant")) return RedirectToAction("Index", "Merchant");
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
