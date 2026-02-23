using System.Diagnostics;
using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Discounts.Web.Models;
using Discounts.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDiscountRepository _discountRepo;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly DiscountsDbContext _context;
    private readonly IGlobalSettingRepository _settingsRepo;

    public HomeController(IDiscountRepository discountRepo, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, DiscountsDbContext context, IGlobalSettingRepository settingsRepo)
    {
        _discountRepo = discountRepo;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _settingsRepo = settingsRepo;
    }

    public async Task<IActionResult> Index(string? message = null)
    {
        var activeDiscounts = await _discountRepo.GetActiveDiscountsAsync().ConfigureAwait(false);
        var viewModels = activeDiscounts.Select(d => new StorefrontViewModel
        {
            Id = d.Id,
            Title = d.Title,
            ImageUrl = d.ImageUrl,
            OriginalPrice = d.OriginalPrice,
            DiscountedPrice = d.DiscountedPrice,
            QuantityAvailable = d.Quantity,

            MerchantName = d.Merchant?.Name ?? "Unknown Merchant",
            CategoryName = d.Category?.Name ?? "General"
        });

        return View(viewModels);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var discount = await _discountRepo.GetByIdAsync(id).ConfigureAwait(false);
        if (discount == null || !discount.IsActive || discount.Quantity <= 0 || discount.EndDate < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "This offer is no longer available.";
            return RedirectToAction(nameof(Index));
        }
        var model = new StorefrontDetailsViewModel
        {
            Id = discount.Id,
            Title = discount.Title,
            Description = discount.Description,
            ImageUrl = discount.ImageUrl,
            OriginalPrice = discount.OriginalPrice,
            DiscountedPrice = discount.DiscountedPrice,
            QuantityAvailable = discount.Quantity,
            EndDate = discount.EndDate,
            MerchantName = discount.Merchant?.Name ?? "Unknown Merchant",
            CategoryName = discount.Category?.Name ?? "General"
        };

        return View(model);
    }
    [HttpPost]
    [Authorize] //this because only logged-in users can reserve
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(int id)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Merchant"))
        {
            TempData["ErrorMessage"] = "Admins and Merchants cannot reserve offers.";
            return RedirectToAction(nameof(Index));
        }
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return Challenge();
        }
        var discount = await _context.Discounts
            .FirstOrDefaultAsync(d => d.Id == id)
            .ConfigureAwait(false);
        if (discount == null || !discount.IsActive || discount.Quantity <= 0 || discount.EndDate < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "We're sorry, this offer just sold out or is no longer available.";
            return RedirectToAction(nameof(Index));
        }
        var settings = await _settingsRepo.GetSettingsAsync().ConfigureAwait(false);
        discount.Quantity -= 1;
        string generatedCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        var reservation = new Reservation
        {
            DiscountId = discount.Id,
            CustomerEmail = user.Email,
            CouponCode = generatedCode,
            ReservationTime = DateTime.UtcNow,
            ExpiryTime = DateTime.UtcNow.AddMinutes(settings.ReservationTimeoutMinutes),
            IsUsed = false,
            IsPaid = false
        };

        _context.Reservations.Add(reservation);
        //crazy save
        await _context.SaveChangesAsync().ConfigureAwait(false);
        TempData["SuccessMessage"] = $"Offer reserved! You have {settings.ReservationTimeoutMinutes} minutes to complete the checkout process before this reservation expires.";
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyCoupons()
    {
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user == null) return Challenge();
        var reservations = await _context.Reservations
            .Include(r => r.Discount)
            .ThenInclude(d => d.Merchant)
            .Where(r => r.CustomerEmail == user.Email)
            .OrderByDescending(r => r.ReservationTime)
            .ToListAsync()
            .ConfigureAwait(false);

        return View(reservations);
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(int reservationId)
    {
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);

        var reservation = await _context.Reservations
            .Include(r => r.Discount)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.CustomerEmail == user.Email)
            .ConfigureAwait(false);
        if (reservation == null) return NotFound();
        if (reservation.IsPaid) return BadRequest("Already purchased.");
        if (reservation.ExpiryTime < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "This reservation has expired.";
            return RedirectToAction(nameof(MyCoupons));
        }
        reservation.IsPaid = true;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        TempData["SuccessMessage"] = "Payment successful! Your coupon code is now unlocked.";
        return RedirectToAction(nameof(MyCoupons));
    }
}
