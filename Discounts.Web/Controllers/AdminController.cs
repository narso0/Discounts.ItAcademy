// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Enums;
using Discounts.Infrastructure.Context;
using Discounts.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DiscountsDbContext _context;
        private readonly IDiscountRepository  _discountRepo;
        public AdminController(DiscountsDbContext context, IDiscountRepository discountRepo)
        {
            _context = context;
            _discountRepo = discountRepo;
        }

        [HttpGet]
        public async Task<IActionResult> PendingMerchants()
        {
            var pendingMerchants = await _context.Merchants
                .Where(m => m.Status == 0)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
            return View(pendingMerchants);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveMerchant(int id)
        {
            var merchant = await _context.Merchants.FindAsync(new object[] { id }).ConfigureAwait(false);
            if (merchant == null)
            {
                return NotFound();
            }

            merchant.Status = MerchantStatus.Active;

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(PendingMerchants));
        }

        [HttpGet]
        public async Task<IActionResult> PendingOffers()
        {
            var pendingDiscounts = await _discountRepo.GetPendingDiscountsAsync().ConfigureAwait(false);
            var viewModels = pendingDiscounts.Select(d => new PendingOfferViewModel
            {
                Id = d.Id,
                Title = d.Title,
                OriginalPrice = d.OriginalPrice,
                DiscountedPrice = d.DiscountedPrice,
                Quantity = d.Quantity,
                CategoryName = d.Category?.Name ?? "Unknown Category",
                MerchantName = d.Merchant?.Name ?? "Unknown Merchant",
                StartDate = d.StartDate,
                EndDate = d.EndDate
            });
            return View(viewModels);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOffer(int id)
        {
            var discount = await _discountRepo.GetByIdAsync(id).ConfigureAwait(false);
            if (discount == null)
            {
                return NotFound("The requested offer could not be found.");
            }
            discount.IsActive = true;
            await _discountRepo.UpdateAsync(discount).ConfigureAwait(false);
            return RedirectToAction(nameof(PendingOffers));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOffer(int id, string rejectionReason)
        {
            var discount = await _discountRepo.GetByIdAsync(id).ConfigureAwait(false);
            if (discount == null)
            {
                return NotFound("The requested offer could not be found.");
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "You must provide a reason for rejection.";
                return RedirectToAction(nameof(PendingOffers));
            }
            discount.IsActive = false;
            discount.RejectionReason = rejectionReason;

            await _discountRepo.UpdateAsync(discount).ConfigureAwait(false);
            TempData["SuccessMessage"] = $"Offer '{discount.Title}' was rejected.";
            return RedirectToAction(nameof(PendingOffers));
        }
    }
}
