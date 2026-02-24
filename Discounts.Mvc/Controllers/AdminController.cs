using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Discounts.Application.Services;
using Discounts.Web.Models.Admin;

namespace Discounts.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }
    [HttpGet]
    public async Task<IActionResult> PendingMerchants()
    {
        var pendingMerchants = await _adminService.GetPendingMerchantsAsync().ConfigureAwait(false);
        return View(pendingMerchants);
    }

    [HttpGet]
    public async Task<IActionResult> PendingOffers()
    {
        var offers = await _adminService.GetPendingOffersAsync().ConfigureAwait(false);
        var viewModels = offers.Select(o => new Discounts.Web.Models.Admin.PendingOfferViewModel
        {
            Id = o.Id,
            Title = o.Title,
            OriginalPrice = o.OriginalPrice,
            DiscountedPrice = o.DiscountedPrice,
            Quantity = o.Quantity,
            CategoryName = o.Category?.Name ?? "N/A",
            MerchantName = o.Merchant?.Name ?? "Unknown",
            StartDate = o.StartDate,
            EndDate = o.EndDate
        });

        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOffer(int id)
    {
        try
        {
            await _adminService.ApproveOfferAsync(id).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Offer approved successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(PendingOffers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOffer(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToAction(nameof(PendingOffers));
        }

        try
        {
            await _adminService.RejectOfferAsync(id, reason).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Offer rejected.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(PendingOffers));
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        var viewModel = new SettingsViewModel
        {
            ReservationTimeoutMinutes = settings?.ReservationTimeoutMinutes ?? 30,
            MerchantEditGracePeriodHours = settings?.MerchantEditGracePeriodHours ?? 24
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        settings.ReservationTimeoutMinutes = model.ReservationTimeoutMinutes;
        settings.MerchantEditGracePeriodHours = model.MerchantEditGracePeriodHours;

        await _adminService.UpdateGlobalSettingsAsync(settings).ConfigureAwait(false);

        TempData["SuccessMessage"] = "Global settings updated successfully.";
        return RedirectToAction(nameof(Settings));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveMerchant(int id)
    {
        try
        {
            await _adminService.ApproveMerchantAsync(id).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Merchant approved successfully. They can now create offers.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error approving merchant: " + ex.Message;
        }
        return RedirectToAction(nameof(PendingMerchants));
    }
}
