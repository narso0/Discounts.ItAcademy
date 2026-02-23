// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

public record ReserveDto(string Email, int TimeoutMinutes);
public record PaymentDto(string Email);
public class DiscountsController : BaseApiController
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDiscounts()
    {
        var discounts = await _discountService.GetActiveDiscountsAsync().ConfigureAwait(false);
        return Ok(discounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscountById(int id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id).ConfigureAwait(false);
        if (discount == null) return NotFound(new { Message = "Discount not found." });

        return Ok(discount);
    }

    [HttpPost("{id}/reserve")]
    public async Task<IActionResult> ReserveDiscount(int id, [FromBody] ReserveDto request)
    {
        // Using your exact service method
        var reservation = await _discountService.ReserveDiscountAsync(id, request.Email, request.TimeoutMinutes).ConfigureAwait(false);
        return Ok(reservation);
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations([FromQuery] string email)
    {
        var reservations = await _discountService.GetCustomerReservationsAsync(email).ConfigureAwait(false);
        return Ok(reservations);
    }

    [HttpPost("reservations/{id}/pay")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentDto request)
    {
        await _discountService.ProcessPaymentAsync(id, request.Email).ConfigureAwait(false);
        return Ok(new { Message = "Payment processed successfully." });
    }
}
