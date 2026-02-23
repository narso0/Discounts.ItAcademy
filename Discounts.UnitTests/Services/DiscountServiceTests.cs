// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Moq;

namespace Discounts.Tests.Services
{
    public class DiscountServiceTests
    {
        [Fact]
        public async Task ReserveDiscountAsync_ValidRequest_DecreasesQuantityAndReturnsReservation()
        {
            var mockRepo = new Mock<IDiscountRepository>();
            var activeDiscount = new Discount
            {
                Id = 1,
                Title = "Test Offer",
                IsActive = true,
                Quantity = 5,
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(1))
                    .ReturnsAsync(activeDiscount);
            var service = new DiscountService(mockRepo.Object);
            var testEmail = "customer@test.com";
            var timeoutMinutes = 30;

            var result = await service.ReserveDiscountAsync(1, testEmail, timeoutMinutes);

            Assert.Equal(4, activeDiscount.Quantity);
            Assert.Equal(testEmail, result.CustomerEmail);
            Assert.False(result.IsPaid);
            Assert.NotNull(result.CouponCode);
            Assert.Equal(8, result.CouponCode.Length);
        }
        [Fact]
        public async Task ReserveDiscountAsync_SoldOut_ThrowsInvalidOperationException()
        {
            var mockRepo = new Mock<IDiscountRepository>();
            var soldOutDiscount = new Discount
            {
                Id = 2,
                IsActive = true,
                Quantity = 0,
                EndDate = DateTime.UtcNow.AddDays(5)
            };
            mockRepo.Setup(repo => repo.GetByIdAsync(2))
                    .ReturnsAsync(soldOutDiscount);
            var service = new DiscountService(mockRepo.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ReserveDiscountAsync(2, "hacker@test.com", 30));
            Assert.Equal("This offer is no longer available.", exception.Message);
        }
    }
}
