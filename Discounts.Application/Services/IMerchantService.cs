// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Services
{
    public interface IMerchantService
    {
        Task<int> GetMerchantIdByEmailAsync(string email);
        Task CreateMerchantProfileAsync(Merchant merchant);
        Task<bool> IsMerchantPendingAsync(string email);
        Task<IEnumerable<Reservation>> GetSalesHistoryAsync(int merchantId);
        Task<IEnumerable<Discount>> GetMerchantDiscountsAsync(int merchantId);
        Task CreateDiscountAsync(Discount discount);
        Task<Discount?> GetMerchantDiscountByIdAsync(int id, int merchantId);
        Task UpdateDiscountAsync(Discount discount);
    }
}
