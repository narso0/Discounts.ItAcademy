// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Repositories;
using Discounts.Web.Models.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly IDiscountRepository _discountRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Infrastructure.Context.DiscountsDbContext _context;

        public MerchantController(IDiscountRepository discountRepo, ICategoryRepository categoryRepo,
            UserManager<IdentityUser> userManager, Infrastructure.Context.DiscountsDbContext context)
        {
            _discountRepo = discountRepo;
            _categoryRepo = categoryRepo;
            _userManager = userManager;
            _context = context;
        }

        private int GetCurrentMerchantId()
        {
            var userEmail = User.Identity.Name;
            var merchant = _context.Merchants.SingleOrDefault(x => x.Email == userEmail);
            if(merchant == null) throw new UnauthorizedAccessException("Merchant not found");
            return merchant.Id;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var discounts = await _discountRepo.GetAllByMerchantAsync(GetCurrentMerchantId()).ConfigureAwait(false);
            var viewModels = discounts.Select(d => new DiscountViewModel
            {
                Id = d.Id,
                Title = d.Title,
                OriginalPrice = d.OriginalPrice,
                DiscountedPrice = d.DiscountedPrice,
                Quantity = d.Quantity,
                IsActive = d.IsActive,
                CategoryName = d.Category?.Name ?? "N/A",
                RejectionReason = d.RejectionReason
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
                MerchantId = GetCurrentMerchantId()
            };
            await _discountRepo.AddAsync(discount).ConfigureAwait(false);
            //never return a view directly form a post
            return RedirectToAction(nameof(Index));
        }
    }
}
