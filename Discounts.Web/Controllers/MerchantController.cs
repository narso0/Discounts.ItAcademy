// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Repositories;
using Discounts.Web.Models.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly IDiscountRepository _discountRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Infrastructure.Context.DiscountsDbContext _context;
        private readonly IGlobalSettingRepository _globalSettingRepo;

        public MerchantController(IDiscountRepository discountRepo, ICategoryRepository categoryRepo,
            UserManager<IdentityUser> userManager, Infrastructure.Context.DiscountsDbContext context, IGlobalSettingRepository globalSettingRepo)
        {
            _discountRepo = discountRepo;
            _categoryRepo = categoryRepo;
            _userManager = userManager;
            _context = context;
            _globalSettingRepo = globalSettingRepo;
        }

        private async Task<int> GetCurrentMerchantId()
        {
            var userEmail = User.Identity?.Name;
            var merchant = await _context.Merchants.SingleOrDefaultAsync(x => x.Email == userEmail).ConfigureAwait(false);
            if(merchant == null) throw new UnauthorizedAccessException("Merchant not found");
            return merchant.Id;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var merchantId = await GetCurrentMerchantId().ConfigureAwait(false);
            var settings = await _globalSettingRepo.GetSettingsAsync().ConfigureAwait(false);
            var discounts = await _discountRepo.GetAllByMerchantAsync(merchantId).ConfigureAwait(false);
            var viewModels = discounts.Select(d => new DiscountViewModel
            {
                Id = d.Id,
                Title = d.Title,
                OriginalPrice = d.OriginalPrice,
                DiscountedPrice = d.DiscountedPrice,
                Quantity = d.Quantity,
                IsActive = d.IsActive,
                CategoryName = d.Category?.Name ?? "N/A",
                RejectionReason = d.RejectionReason,
                CanEdit = (DateTime.UtcNow - d.CreatedAt).TotalHours <= settings.MerchantEditGracePeriodHours
            });

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var categories = await _categoryRepo.GetAllAsync().ConfigureAwait(false);
            var viewModel = new DiscountCreateViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepo.GetAllAsync().ConfigureAwait(false);
                model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(model);
            }
            var merchantId = await GetCurrentMerchantId().ConfigureAwait(false);
            var discount = new Discount
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                OriginalPrice = model.OriginalPrice,
                DiscountedPrice = model.DiscountedPrice,
                Quantity = model.Quantity,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CategoryId = model.CategoryId,
                MerchantId = merchantId
            };
            await _discountRepo.AddAsync(discount).ConfigureAwait(false);
            //never return a view directly form a post
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var merchantId = await GetCurrentMerchantId().ConfigureAwait(false);
            var discount = await _discountRepo.GetByIdAsync(id).ConfigureAwait(false);
            if (discount == null || discount.MerchantId != merchantId)
            {
                return NotFound("Merchant or Offer not found");
            }

            var settings = await _globalSettingRepo.GetSettingsAsync().ConfigureAwait(false);
            if ((DateTime.UtcNow - discount.CreatedAt).TotalHours > settings.MerchantEditGracePeriodHours)
            {
                TempData["ErrorMessage"] = "The edit grace period for this offer has expired.";
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepo.GetAllAsync().ConfigureAwait(false);
            var model = new DiscountEditViewModel
            {
                Id = discount.Id,
                Title = discount.Title,
                Description = discount.Description,
                ImageUrl = discount.ImageUrl,
                OriginalPrice = discount.OriginalPrice,
                DiscountedPrice = discount.DiscountedPrice,
                Quantity = discount.Quantity,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                CategoryId = discount.CategoryId,
                Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiscountEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepo.GetAllAsync().ConfigureAwait(false);
                model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(model);
            }

            var merchantId = await GetCurrentMerchantId().ConfigureAwait(false);
            var discount = await _discountRepo.GetByIdAsync(model.Id).ConfigureAwait(false);

            if (discount == null || discount.MerchantId != merchantId)
            {
                return NotFound("Offer not found or unauthorized.");
            }

            var settings = await _globalSettingRepo.GetSettingsAsync().ConfigureAwait(false);
            if ((DateTime.UtcNow - discount.CreatedAt).TotalHours > settings.MerchantEditGracePeriodHours)
            {
                TempData["ErrorMessage"] = "The edit grace period for this offer has expired.";
                return RedirectToAction(nameof(Index));
            }
            discount.Title = model.Title;
            discount.Description = model.Description;
            discount.ImageUrl = model.ImageUrl;
            discount.OriginalPrice = model.OriginalPrice;
            discount.DiscountedPrice = model.DiscountedPrice;
            discount.Quantity = model.Quantity;
            discount.StartDate = model.StartDate;
            discount.EndDate = model.EndDate;
            discount.CategoryId = model.CategoryId;

            discount.IsActive = false;
            discount.RejectionReason = null;

            await _discountRepo.UpdateAsync(discount).ConfigureAwait(false);

            TempData["SuccessMessage"] = "Offer updated successfully and sent to Admin for review.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> SalesHistory()
        {
            var merchantId = await GetCurrentMerchantId().ConfigureAwait(false);
            var sales = await _context.Reservations
                .Include(r => r.Discount)
                .Where(r => r.Discount!.MerchantId == merchantId && r.IsPaid == true)
                .OrderByDescending(r => r.ReservationTime)
                .Select(r => new SalesHistoryViewModel
                {
                    DiscountTitle = r.Discount!.Title,
                    CustomerEmail = r.CustomerEmail,
                    TransactionDate = r.ReservationTime,
                    CouponCode = r.CouponCode,
                    PricePaid = r.Discount.DiscountedPrice
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return View(sales);
        }
    }
}
