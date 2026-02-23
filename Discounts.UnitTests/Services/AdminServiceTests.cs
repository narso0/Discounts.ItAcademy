// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Discounts.Domain.Enums;
using Xunit;

namespace Discounts.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IDiscountRepository> _mockDiscountRepo;
        private readonly Mock<IGlobalSettingRepository> _mockSettingsRepo;
        private readonly Mock<IMerchantRepository> _mockMerchantRepo;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _mockDiscountRepo = new Mock<IDiscountRepository>();
            _mockSettingsRepo = new Mock<IGlobalSettingRepository>();
            _mockMerchantRepo = new Mock<IMerchantRepository>();
            _service = new AdminService(_mockDiscountRepo.Object, _mockSettingsRepo.Object, _mockMerchantRepo.Object);
        }

        [Fact]
        public async Task ApproveOfferAsync_OfferExists_SetsActiveAndClearsReason()
        {
            var discount = new Discount { Id = 1, IsActive = false, RejectionReason = "Too blurry" };
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(discount);

            await _service.ApproveOfferAsync(1);

            Assert.True(discount.IsActive);
            Assert.Null(discount.RejectionReason);
            _mockDiscountRepo.Verify(r => r.UpdateAsync(discount), Times.Once);
        }

        [Fact]
        public async Task ApproveOfferAsync_OfferNotFound_ThrowsInvalidOperation()
        {
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Discount?)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApproveOfferAsync(1));
        }

        [Fact]
        public async Task RejectOfferAsync_OfferExists_SetsInactiveAndSavesReason()
        {
            var discount = new Discount { Id = 1, IsActive = true, RejectionReason = null };
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(discount);

            await _service.RejectOfferAsync(1, "Image violates guidelines");

            Assert.False(discount.IsActive);
            Assert.Equal("Image violates guidelines", discount.RejectionReason);
            _mockDiscountRepo.Verify(r => r.UpdateAsync(discount), Times.Once);
        }

        [Fact]
        public async Task RejectOfferAsync_OfferNotFound_ThrowsInvalidOperation()
        {
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Discount?)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RejectOfferAsync(1, "Reason"));
        }

        [Fact]
        public async Task UpdateGlobalSettingsAsync_CallsRepositoryUpdate()
        {
            var settings = new GlobalSetting { ReservationTimeoutMinutes = 45 };

            await _service.UpdateGlobalSettingsAsync(settings);

            _mockSettingsRepo.Verify(r => r.UpdateSettingsAsync(settings), Times.Once);
        }
        [Fact]
        public async Task GetPendingOffersAsync_ReturnsPendingDiscounts()
        {
            var pendingOffers = new List<Discount> { new Discount { Id = 1, IsActive = false } };
            _mockDiscountRepo.Setup(r => r.GetPendingDiscountsAsync()).ReturnsAsync(pendingOffers);

            var result = await _service.GetPendingOffersAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPendingMerchantsAsync_ReturnsPendingMerchants()
        {
            var pendingMerchants = new List<Merchant> { new Merchant { Id = 1, Status = MerchantStatus.Pending } };
            _mockMerchantRepo.Setup(r => r.GetPendingMerchantsAsync()).ReturnsAsync(pendingMerchants);

            var result = await _service.GetPendingMerchantsAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetGlobalSettingsAsync_ReturnsSettings()
        {
            var settings = new GlobalSetting { ReservationTimeoutMinutes = 60 };
            _mockSettingsRepo.Setup(r => r.GetSettingsAsync()).ReturnsAsync(settings);

            var result = await _service.GetGlobalSettingsAsync();

            Assert.NotNull(result);
            Assert.Equal(60, result.ReservationTimeoutMinutes);
        }
    }
}
