// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;
using Discounts.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DiscountsDbContext _context;
        public AdminController(DiscountsDbContext context)
        {
            _context = context;
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
    }
}
