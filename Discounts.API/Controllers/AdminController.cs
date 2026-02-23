// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

public record RejectOfferDto(string Reason);

public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("merchants/pending")]
    public async Task<IActionResult> GetPendingMerchants()
    {
        var merchants = await _adminService.GetPendingMerchantsAsync().ConfigureAwait(false);
        return Ok(merchants);
    }

    [HttpGet("offers/pending")]
    public async Task<IActionResult> GetPendingOffers()
    {
        var offers = await _adminService.GetPendingOffersAsync().ConfigureAwait(false);
        return Ok(offers);
    }

    [HttpPost("offers/{id}/approve")]
    public async Task<IActionResult> ApproveOffer(int id)
    {
        await _adminService.ApproveOfferAsync(id).ConfigureAwait(false);
        return Ok(new { Message = $"Offer {id} approved and activated." });
    }

    [HttpPost("offers/{id}/reject")]
    public async Task<IActionResult> RejectOffer(int id, [FromBody] RejectOfferDto request)
    {
        await _adminService.RejectOfferAsync(id, request.Reason).ConfigureAwait(false);
        return Ok(new { Message = $"Offer {id} rejected." });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetGlobalSettings()
    {
        var settings = await _adminService.GetGlobalSettingsAsync().ConfigureAwait(false);
        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateGlobalSettings([FromBody] GlobalSetting settings)
    {
        await _adminService.UpdateGlobalSettingsAsync(settings).ConfigureAwait(false);
        return Ok(new { Message = "Global settings updated." });
    }
}
