// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Discounts.Tests.Services
{
    public class DiscountServiceTests
    {
        private readonly Mock<IDiscountRepository> _mockRepo;
        private readonly DiscountService _service;

        public DiscountServiceTests()
        {
            _mockRepo = new Mock<IDiscountRepository>();
            _service = new DiscountService(_mockRepo.Object);
        }

        [Fact]
        public async Task ReserveDiscountAsync_ValidRequest_DecreasesQuantityAndReturnsReservation()
        {
            var activeDiscount = new Discount { Id = 1, IsActive = true, Quantity = 5, EndDate = DateTime.UtcNow.AddDays(5) };
            _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(activeDiscount);

            var result = await _service.ReserveDiscountAsync(1, "customer@test.com", 30);

            Assert.Equal(4, activeDiscount.Quantity);
            Assert.Equal("customer@test.com", result.CustomerEmail);
            Assert.False(result.IsPaid);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Discount>()), Times.Once);
            _mockRepo.Verify(r => r.AddReservationAsync(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task ReserveDiscountAsync_SoldOut_ThrowsInvalidOperationException()
        {
            var soldOutDiscount = new Discount { Id = 2, IsActive = true, Quantity = 0, EndDate = DateTime.UtcNow.AddDays(5) };
            _mockRepo.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(soldOutDiscount);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReserveDiscountAsync(2, "test@test.com", 30));
            Assert.Equal("This offer is no longer available.", ex.Message);
        }

        [Fact]
        public async Task ReserveDiscountAsync_Expired_ThrowsInvalidOperationException()
        {
            var expiredDiscount = new Discount { Id = 3, IsActive = true, Quantity = 10, EndDate = DateTime.UtcNow.AddDays(-1) };
            _mockRepo.Setup(repo => repo.GetByIdAsync(3)).ReturnsAsync(expiredDiscount);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReserveDiscountAsync(3, "test@test.com", 30));
        }

        [Fact]
        public async Task ReserveDiscountAsync_NotActive_ThrowsInvalidOperationException()
        {
            var inactiveDiscount = new Discount { Id = 4, IsActive = false, Quantity = 10, EndDate = DateTime.UtcNow.AddDays(5) };
            _mockRepo.Setup(repo => repo.GetByIdAsync(4)).ReturnsAsync(inactiveDiscount);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReserveDiscountAsync(4, "test@test.com", 30));
        }

        [Fact]
        public async Task ProcessPaymentAsync_ValidReservation_SetsIsPaidToTrue()
        {
            var reservation = new Reservation { Id = 1, IsPaid = false, ExpiryTime = DateTime.UtcNow.AddMinutes(10) };
            _mockRepo.Setup(r => r.GetReservationByIdAndEmailAsync(1, "test@test.com")).ReturnsAsync(reservation);

            await _service.ProcessPaymentAsync(1, "test@test.com");

            Assert.True(reservation.IsPaid);
            _mockRepo.Verify(r => r.UpdateReservationAsync(reservation), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetReservationByIdAndEmailAsync(1, "test@test.com")).ReturnsAsync((Reservation?)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ProcessPaymentAsync(1, "test@test.com"));
        }

        [Fact]
        public async Task ProcessPaymentAsync_AlreadyPaid_ThrowsInvalidOperationException()
        {
            var reservation = new Reservation { Id = 1, IsPaid = true, ExpiryTime = DateTime.UtcNow.AddMinutes(10) };
            _mockRepo.Setup(r => r.GetReservationByIdAndEmailAsync(1, "test@test.com")).ReturnsAsync(reservation);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(1, "test@test.com"));
            Assert.Equal("Already purchased.", ex.Message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ExpiredReservation_ThrowsInvalidOperationException()
        {
            var reservation = new Reservation { Id = 1, IsPaid = false, ExpiryTime = DateTime.UtcNow.AddMinutes(-5) };
            _mockRepo.Setup(r => r.GetReservationByIdAndEmailAsync(1, "test@test.com")).ReturnsAsync(reservation);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(1, "test@test.com"));
            Assert.Equal("This reservation has expired.", ex.Message);
        }
        [Fact]
        public async Task GetActiveDiscountsAsync_ReturnsListOfDiscounts()
        {
            var expectedDiscounts = new List<Discount> { new Discount { Id = 1 }, new Discount { Id = 2 } };
            _mockRepo.Setup(r => r.GetActiveDiscountsAsync()).ReturnsAsync(expectedDiscounts);

            var result = await _service.GetActiveDiscountsAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetDiscountByIdAsync_Exists_ReturnsDiscount()
        {
            var expectedDiscount = new Discount { Id = 5 };
            _mockRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(expectedDiscount);

            var result = await _service.GetDiscountByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task GetCustomerReservationsAsync_ReturnsCustomerReservations()
        {
            var expectedReservations = new List<Reservation> { new Reservation { Id = 1 }, new Reservation { Id = 2 } };
            _mockRepo.Setup(r => r.GetCustomerReservationsAsync("test@test.com")).ReturnsAsync(expectedReservations);

            var result = await _service.GetCustomerReservationsAsync("test@test.com");

            Assert.Equal(2, result.Count());
        }
    }
}
