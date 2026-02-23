using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Discounts.Web.Models.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discounts.Web.Controllers;

[Authorize(Roles = "Merchant")]
public class MerchantController : Controller
{
    private readonly IMerchantService _merchantService;
    private readonly ICategoryService _categoryService;
    private readonly IAdminService _adminService;

    public MerchantController(IMerchantService merchantService, ICategoryService categoryService, IAdminService adminService)
    {
        _merchantService = merchantService;
        _categoryService = categoryService;
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var merchantId = await _merchantService.GetMerchantIdByEmailAsync(User.Identity!.Name!).ConfigureAwait(false);
        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        var discounts = await _merchantService.GetMerchantDiscountsAsync(merchantId).ConfigureAwait(false);

        var viewModels = discounts.Select(d => new DiscountViewModel
        {
            Id = d.Id, Title = d.Title, OriginalPrice = d.OriginalPrice, DiscountedPrice = d.DiscountedPrice,
            Quantity = d.Quantity, IsActive = d.IsActive, CategoryName = d.Category?.Name ?? "N/A", RejectionReason = d.RejectionReason,
            CanEdit = (DateTime.UtcNow - d.CreatedAt).TotalHours <= settings.MerchantEditGracePeriodHours
        });
        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int id)
    {
        var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
        return View(new DiscountCreateViewModel { Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiscountCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
            model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(model);
        }
        var merchantId = await _merchantService.GetMerchantIdByEmailAsync(User.Identity!.Name!).ConfigureAwait(false);
        await _merchantService.CreateDiscountAsync(new Discount
        {
            Title = model.Title, Description = model.Description, ImageUrl = model.ImageUrl, OriginalPrice = model.OriginalPrice,
            DiscountedPrice = model.DiscountedPrice, Quantity = model.Quantity, StartDate = model.StartDate,
            EndDate = model.EndDate, CategoryId = model.CategoryId, MerchantId = merchantId
        }).ConfigureAwait(false);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var merchantId = await _merchantService.GetMerchantIdByEmailAsync(User.Identity!.Name!).ConfigureAwait(false);
        var discount = await _merchantService.GetMerchantDiscountByIdAsync(id, merchantId).ConfigureAwait(false);

        if (discount == null) return NotFound("Merchant or Offer not found");

        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        if ((DateTime.UtcNow - discount.CreatedAt).TotalHours > settings.MerchantEditGracePeriodHours)
        {
            TempData["ErrorMessage"] = "The edit grace period for this offer has expired.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
        return View(new DiscountEditViewModel
        {
            Id = discount.Id, Title = discount.Title, Description = discount.Description, ImageUrl = discount.ImageUrl,
            OriginalPrice = discount.OriginalPrice, DiscountedPrice = discount.DiscountedPrice, Quantity = discount.Quantity,
            StartDate = discount.StartDate, EndDate = discount.EndDate, CategoryId = discount.CategoryId,
            Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DiscountEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
            model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(model);
        }

        var merchantId = await _merchantService.GetMerchantIdByEmailAsync(User.Identity!.Name!).ConfigureAwait(false);
        var discount = await _merchantService.GetMerchantDiscountByIdAsync(model.Id, merchantId).ConfigureAwait(false);

        if (discount == null) return NotFound("Offer not found or unauthorized.");

        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        if ((DateTime.UtcNow - discount.CreatedAt).TotalHours > settings.MerchantEditGracePeriodHours)
        {
            TempData["ErrorMessage"] = "The edit grace period for this offer has expired.";
            return RedirectToAction(nameof(Index));
        }

        discount.Title = model.Title; discount.Description = model.Description; discount.ImageUrl = model.ImageUrl;
        discount.OriginalPrice = model.OriginalPrice; discount.DiscountedPrice = model.DiscountedPrice;
        discount.Quantity = model.Quantity; discount.StartDate = model.StartDate; discount.EndDate = model.EndDate;
        discount.CategoryId = model.CategoryId; discount.IsActive = false; discount.RejectionReason = null;

        await _merchantService.UpdateDiscountAsync(discount).ConfigureAwait(false);
        TempData["SuccessMessage"] = "Offer updated successfully and sent to Admin for review.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SalesHistory()
    {
        var merchantId = await _merchantService.GetMerchantIdByEmailAsync(User.Identity!.Name!).ConfigureAwait(false);
        var sales = await _merchantService.GetSalesHistoryAsync(merchantId).ConfigureAwait(false);

        return View(sales.Select(r => new SalesHistoryViewModel
        {
            DiscountTitle = r.Discount!.Title, CustomerEmail = r.CustomerEmail, TransactionDate = r.ReservationTime,
            CouponCode = r.CouponCode, PricePaid = r.Discount.DiscountedPrice
        }));
    }
}
