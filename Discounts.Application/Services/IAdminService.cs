// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<Discount>> GetPendingOffersAsync();
        Task ApproveOfferAsync(int discountId);
        Task RejectOfferAsync(int discountId, string reason);
        Task<GlobalSetting> GetGlobalSettingsAsync();
        Task UpdateGlobalSettingsAsync(GlobalSetting settings);
        Task<IEnumerable<Merchant>> GetPendingMerchantsAsync();
    }
}
