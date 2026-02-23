// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Interfaces
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllByMerchantAsync(int merchantId);
        Task<Discount?> GetByIdAsync(int id);
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(Discount discount);

        //added later
        Task<IEnumerable<Discount>> GetPendingDiscountsAsync();
        //new
        Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
        Task AddReservationAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetCustomerReservationsAsync(string email);
        Task<Reservation?> GetReservationByIdAndEmailAsync(int id, string email);
        Task UpdateReservationAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetPaidSalesByMerchantAsync(int merchantId);
    }
}
