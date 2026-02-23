// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Services
{
    public interface IDiscountService
    {
        Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task<Reservation> ReserveDiscountAsync(int discountId, string userEmail, int timeoutMinutes);
        Task<IEnumerable<Reservation>> GetCustomerReservationsAsync(string email);
        Task ProcessPaymentAsync(int reservationId, string email);

    }
}
