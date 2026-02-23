// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;

namespace Discounts.Application.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepository _merchantRepo;
        private readonly IDiscountRepository _discountRepo;

        public MerchantService(IMerchantRepository merchantRepo, IDiscountRepository discountRepo)
        {
            _merchantRepo = merchantRepo;
            _discountRepo = discountRepo;
        }

        public async Task<int> GetMerchantIdByEmailAsync(string email)
        {
            var merchant = await _merchantRepo.GetByEmailAsync(email);
            if (merchant == null) throw new UnauthorizedAccessException("Merchant not found");
            return merchant.Id;
        }

        public async Task CreateMerchantProfileAsync(Merchant merchant)
        {
            merchant.Status = MerchantStatus.Pending;
            merchant.CreatedAt = DateTime.UtcNow;
            await _merchantRepo.AddAsync(merchant);
        }

        public async Task<bool> IsMerchantPendingAsync(string email)
        {
            var merchant = await _merchantRepo.GetByEmailAsync(email);
            return merchant != null && merchant.Status == MerchantStatus.Pending;
        }

        public async Task<IEnumerable<Reservation>> GetSalesHistoryAsync(int merchantId) => await _discountRepo.GetPaidSalesByMerchantAsync(merchantId);

        public async Task<IEnumerable<Discount>> GetMerchantDiscountsAsync(int merchantId) => await _discountRepo.GetAllByMerchantAsync(merchantId);

        public async Task CreateDiscountAsync(Discount discount) => await _discountRepo.AddAsync(discount);

        public async Task<Discount?> GetMerchantDiscountByIdAsync(int id, int merchantId)
        {
            var discount = await _discountRepo.GetByIdAsync(id);
            if (discount == null || discount.MerchantId != merchantId) return null;
            return discount;
        }

        public async Task UpdateDiscountAsync(Discount discount) => await _discountRepo.UpdateAsync(discount);
    }
}
