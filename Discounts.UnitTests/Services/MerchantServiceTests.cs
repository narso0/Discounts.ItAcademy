// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Discounts.Tests.Services
{
    public class MerchantServiceTests
    {
        private readonly Mock<IMerchantRepository> _mockMerchantRepo;
        private readonly Mock<IDiscountRepository> _mockDiscountRepo;
        private readonly MerchantService _service;

        public MerchantServiceTests()
        {
            _mockMerchantRepo = new Mock<IMerchantRepository>();
            _mockDiscountRepo = new Mock<IDiscountRepository>();
            _service = new MerchantService(_mockMerchantRepo.Object, _mockDiscountRepo.Object);
        }

        [Fact]
        public async Task GetMerchantIdByEmailAsync_Exists_ReturnsId()
        {
            _mockMerchantRepo.Setup(r => r.GetByEmailAsync("merch@test.com")).ReturnsAsync(new Merchant { Id = 99 });
            var id = await _service.GetMerchantIdByEmailAsync("merch@test.com");
            Assert.Equal(99, id);
        }

        [Fact]
        public async Task GetMerchantIdByEmailAsync_NotFound_ThrowsUnauthorized()
        {
            _mockMerchantRepo.Setup(r => r.GetByEmailAsync("merch@test.com")).ReturnsAsync((Merchant?)null);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetMerchantIdByEmailAsync("merch@test.com"));
        }

        [Fact]
        public async Task CreateMerchantProfileAsync_SetsStatusPendingAndSaves()
        {
            var merchant = new Merchant();
            await _service.CreateMerchantProfileAsync(merchant);

            Assert.Equal(MerchantStatus.Pending, merchant.Status);
            _mockMerchantRepo.Verify(r => r.AddAsync(merchant), Times.Once);
        }

        [Fact]
        public async Task IsMerchantPendingAsync_IsPending_ReturnsTrue()
        {
            _mockMerchantRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(new Merchant { Status = MerchantStatus.Pending });
            var result = await _service.IsMerchantPendingAsync("test@test.com");
            Assert.True(result);
        }

        [Fact]
        public async Task IsMerchantPendingAsync_IsActive_ReturnsFalse()
        {
            _mockMerchantRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(new Merchant { Status = MerchantStatus.Active });
            var result = await _service.IsMerchantPendingAsync("test@test.com");
            Assert.False(result);
        }

        [Fact]
        public async Task GetMerchantDiscountByIdAsync_BelongsToMerchant_ReturnsDiscount()
        {
            var discount = new Discount { Id = 1, MerchantId = 5 };
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(discount);

            var result = await _service.GetMerchantDiscountByIdAsync(1, 5);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetMerchantDiscountByIdAsync_WrongMerchant_ReturnsNull()
        {
            var discount = new Discount { Id = 1, MerchantId = 10 }; // Belongs to someone else
            _mockDiscountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(discount);

            var result = await _service.GetMerchantDiscountByIdAsync(1, 5);
            Assert.Null(result);
        }
        [Fact]
        public async Task GetSalesHistoryAsync_ReturnsPaidReservations()
        {
            var sales = new List<Reservation> { new Reservation { Id = 1 } };
            _mockDiscountRepo.Setup(r => r.GetPaidSalesByMerchantAsync(10)).ReturnsAsync(sales);

            var result = await _service.GetSalesHistoryAsync(10);

            Assert.Single(result); // Asserts that exactly 1 item is returned
        }

        [Fact]
        public async Task GetMerchantDiscountsAsync_ReturnsAllMerchantsDiscounts()
        {
            var discounts = new List<Discount> { new Discount { Id = 1 }, new Discount { Id = 2 } };
            _mockDiscountRepo.Setup(r => r.GetAllByMerchantAsync(10)).ReturnsAsync(discounts);

            var result = await _service.GetMerchantDiscountsAsync(10);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateDiscountAsync_CallsRepositoryAdd()
        {
            var discount = new Discount { Title = "New Offer" };

            await _service.CreateDiscountAsync(discount);

            _mockDiscountRepo.Verify(r => r.AddAsync(discount), Times.Once);
        }

        [Fact]
        public async Task UpdateDiscountAsync_CallsRepositoryUpdate()
        {
            var discount = new Discount { Id = 1, Title = "Updated Offer" };

            await _service.UpdateDiscountAsync(discount);

            _mockDiscountRepo.Verify(r => r.UpdateAsync(discount), Times.Once);
        }
    }
}
