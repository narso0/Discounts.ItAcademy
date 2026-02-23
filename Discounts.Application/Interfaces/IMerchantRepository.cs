// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Interfaces
{
    public interface IMerchantRepository
    {
        Task<IEnumerable<Merchant>> GetPendingMerchantsAsync();
        Task<Merchant?> GetByEmailAsync(string email);
        Task AddAsync(Merchant merchant);
    }
}
