// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;

namespace Discounts.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IDiscountRepository _discountRepo;
        private readonly IGlobalSettingRepository _settingsRepo;
        private readonly IMerchantRepository _merchantRepo;

        public AdminService(IDiscountRepository discountRepo, IGlobalSettingRepository settingsRepo, IMerchantRepository merchantRepo)
        {
            _discountRepo = discountRepo;
            _settingsRepo = settingsRepo;
            _merchantRepo = merchantRepo;
        }

        public async Task<IEnumerable<Discount>> GetPendingOffersAsync()
        {
            return await _discountRepo.GetPendingDiscountsAsync();
        }

        public async Task ApproveOfferAsync(int discountId)
        {
            var discount = await _discountRepo.GetByIdAsync(discountId);
            if (discount == null) throw new InvalidOperationException("Offer not found.");

            discount.IsActive = true;
            discount.RejectionReason = null;
            await _discountRepo.UpdateAsync(discount);
        }

        public async Task RejectOfferAsync(int discountId, string reason)
        {
            var discount = await _discountRepo.GetByIdAsync(discountId);
            if (discount == null) throw new InvalidOperationException("Offer not found.");

            discount.IsActive = false;
            discount.RejectionReason = reason;
            await _discountRepo.UpdateAsync(discount);
        }

        public async Task<GlobalSetting> GetGlobalSettingsAsync()
        {
            return await _settingsRepo.GetSettingsAsync();
        }

        public async Task UpdateGlobalSettingsAsync(GlobalSetting settings)
        {
            await _settingsRepo.UpdateSettingsAsync(settings);
        }
        public async Task<IEnumerable<Merchant>> GetPendingMerchantsAsync()
        {
            return await _merchantRepo.GetPendingMerchantsAsync();
        }
    }
}
