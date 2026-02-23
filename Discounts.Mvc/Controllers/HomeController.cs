using System.Diagnostics;
using Discounts.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Discounts.Web.Models;
using Discounts.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;

namespace Discounts.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDiscountService _discountService;
    private readonly IAdminService _adminService;

    public HomeController(IDiscountService discountService, IAdminService adminService)
    {
        _discountService = discountService;
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string? message = null)
    {
        var activeDiscounts = await _discountService.GetActiveDiscountsAsync().ConfigureAwait(false);
        var viewModels = activeDiscounts.Select(d => new StorefrontViewModel
        {
            Id = d.Id, Title = d.Title, ImageUrl = d.ImageUrl, OriginalPrice = d.OriginalPrice,
            DiscountedPrice = d.DiscountedPrice, QuantityAvailable = d.Quantity,
            MerchantName = d.Merchant?.Name ?? "Unknown Merchant", CategoryName = d.Category?.Name ?? "General"
        });
        return View(viewModels);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id).ConfigureAwait(false);
        if (discount == null || !discount.IsActive || discount.Quantity <= 0 || discount.EndDate < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "This offer is no longer available.";
            return RedirectToAction(nameof(Index));
        }
        return View(new StorefrontDetailsViewModel
        {
            Id = discount.Id, Title = discount.Title, Description = discount.Description, ImageUrl = discount.ImageUrl,
            OriginalPrice = discount.OriginalPrice, DiscountedPrice = discount.DiscountedPrice, QuantityAvailable = discount.Quantity,
            EndDate = discount.EndDate, MerchantName = discount.Merchant?.Name ?? "Unknown Merchant", CategoryName = discount.Category?.Name ?? "General"
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(int id)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Merchant"))
        {
            TempData["ErrorMessage"] = "Admins and Merchants cannot reserve offers.";
            return RedirectToAction(nameof(Index));
        }
        var email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return Challenge();

        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);

        try
        {
            await _discountService.ReserveDiscountAsync(id, email, settings.ReservationTimeoutMinutes).ConfigureAwait(false);
            TempData["SuccessMessage"] = $"Offer reserved! You have {settings.ReservationTimeoutMinutes} minutes to complete purchase.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyCoupons()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return Challenge();
        var reservations = await _discountService.GetCustomerReservationsAsync(email).ConfigureAwait(false);
        return View(reservations);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(int reservationId)
    {
        try
        {
            await _discountService.ProcessPaymentAsync(reservationId, User.Identity!.Name!).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Payment successful! Your coupon code is now unlocked.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(MyCoupons));
    }
}
