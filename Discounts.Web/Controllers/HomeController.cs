using System.Diagnostics;
using Discounts.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Discounts.Web.Models;
using Discounts.Web.Models.Home;

namespace Discounts.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDiscountRepository _discountRepo;

    public HomeController(IDiscountRepository discountRepo)
    {
        _discountRepo = discountRepo;
    }

    public async Task<IActionResult> Index(string? message = null)
    {
        var activeDiscounts = await _discountRepo.GetActiveDiscountsAsync().ConfigureAwait(false);
        var viewModels = activeDiscounts.Select(d => new StoreFrontViewModel
        {
            Id = d.Id,
            Title = d.Title,
            ImageUrl = d.ImageUrl,
            OriginalPrice = d.OriginalPrice,
            DiscountedPrice = d.DiscountedPrice,
            QuantityAvailable = d.Quantity,

            // Defensive navigation in case a relationship is missing
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
}
