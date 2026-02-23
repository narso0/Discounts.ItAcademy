// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;

namespace Discounts.Application.Services
{
    public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepo;

    public DiscountService(IDiscountRepository discountRepo) => _discountRepo = discountRepo;

    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync() => await _discountRepo.GetActiveDiscountsAsync();

    public async Task<Discount?> GetDiscountByIdAsync(int id) => await _discountRepo.GetByIdAsync(id);

    public async Task<Reservation> ReserveDiscountAsync(int discountId, string userEmail, int timeoutMinutes)
    {
        var discount = await _discountRepo.GetByIdAsync(discountId);
        if (discount == null || !discount.IsActive || discount.Quantity <= 0 || discount.EndDate < DateTime.UtcNow)
            throw new InvalidOperationException("This offer is no longer available.");

        discount.Quantity -= 1;
        var reservation = new Reservation
        {
            DiscountId = discount.Id, CustomerEmail = userEmail,
            CouponCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            ReservationTime = DateTime.UtcNow, ExpiryTime = DateTime.UtcNow.AddMinutes(timeoutMinutes),
            IsUsed = false, IsPaid = false
        };

        await _discountRepo.UpdateAsync(discount);
        await _discountRepo.AddReservationAsync(reservation); // Save is handled here!
        return reservation;
    }

    public async Task<IEnumerable<Reservation>> GetCustomerReservationsAsync(string email) =>
        await _discountRepo.GetCustomerReservationsAsync(email);

    public async Task ProcessPaymentAsync(int reservationId, string email)
    {
        var reservation = await _discountRepo.GetReservationByIdAndEmailAsync(reservationId, email);
        if (reservation == null) throw new KeyNotFoundException("Reservation not found.");
        if (reservation.IsPaid) throw new InvalidOperationException("Already purchased.");
        if (reservation.ExpiryTime < DateTime.UtcNow) throw new InvalidOperationException("This reservation has expired.");

        reservation.IsPaid = true;
        await _discountRepo.UpdateReservationAsync(reservation);
    }
}
}
