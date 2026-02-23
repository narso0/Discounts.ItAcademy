// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

public class MerchantController : BaseApiController
{
    private readonly IMerchantService _merchantService;

    public MerchantController(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] Merchant merchant)
    {
        await _merchantService.CreateMerchantProfileAsync(merchant).ConfigureAwait(false);
        return Ok(new { Message = "Merchant profile created and pending approval." });
    }

    [HttpGet("status")]
    public async Task<IActionResult> CheckStatus([FromQuery] string email)
    {
        var isPending = await _merchantService.IsMerchantPendingAsync(email).ConfigureAwait(false);
        return Ok(new { Email = email, IsPending = isPending });
    }

    [HttpGet("{merchantId}/sales")]
    public async Task<IActionResult> GetSalesHistory(int merchantId)
    {
        var sales = await _merchantService.GetSalesHistoryAsync(merchantId).ConfigureAwait(false);
        return Ok(sales);
    }

    [HttpGet("{merchantId}/discounts")]
    public async Task<IActionResult> GetDiscounts(int merchantId)
    {
        var discounts = await _merchantService.GetMerchantDiscountsAsync(merchantId).ConfigureAwait(false);
        return Ok(discounts);
    }

    [HttpPost("{merchantId}/discounts")]
    public async Task<IActionResult> CreateDiscount(int merchantId, [FromBody] Discount discount)
    {
        discount.MerchantId = merchantId; // Ensure it belongs to them
        await _merchantService.CreateDiscountAsync(discount).ConfigureAwait(false);
        return Ok(new { Message = "Discount created." });
    }

    [HttpPut("{merchantId}/discounts/{discountId}")]
    public async Task<IActionResult> UpdateDiscount(int merchantId, int discountId, [FromBody] Discount discount)
    {
        var existing = await _merchantService.GetMerchantDiscountByIdAsync(discountId, merchantId).ConfigureAwait(false);
        if (existing == null) return NotFound("Discount not found or unauthorized.");

        discount.Id = discountId;
        discount.MerchantId = merchantId;
        await _merchantService.UpdateDiscountAsync(discount).ConfigureAwait(false);
        return Ok(new { Message = "Discount updated." });
    }
}
