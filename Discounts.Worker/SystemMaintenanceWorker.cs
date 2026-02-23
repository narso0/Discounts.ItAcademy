// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Worker
{
    public class SystemMaintenanceWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SystemMaintenanceWorker> _logger;

        public SystemMaintenanceWorker(IServiceProvider serviceProvider, ILogger<SystemMaintenanceWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                await PerformMaintenanceAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
            }
        }
        private async Task PerformMaintenanceAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DiscountsDbContext>();

            try
            {
                bool changesMade = false;

                //reason for existence N1: expired reservations
                var expiredReservations = await context.Reservations
                    .Include(r => r.Discount)
                    .Where(r => !r.IsPaid && r.ExpiryTime < DateTime.UtcNow)
                    .ToListAsync(stoppingToken)
                    .ConfigureAwait(false);

                if (expiredReservations.Any())
                {
                    foreach (var reservation in expiredReservations)
                    {
                        if (reservation.Discount != null)
                        {
                            reservation.Discount.Quantity += 1;
                        }
                    }
                    context.Reservations.RemoveRange(expiredReservations);
                    _logger.LogInformation($"Cleaned up {expiredReservations.Count} abandoned reservations.");
                    changesMade = true;
                }

                //reason for existence N2: expired coupons or offers
                var expiredOffers = await context.Discounts
                    .Where(d => d.IsActive && d.EndDate < DateTime.UtcNow)
                    .ToListAsync(stoppingToken)
                    .ConfigureAwait(false);

                if (expiredOffers.Any())
                {
                    foreach (var offer in expiredOffers)
                    {
                        offer.IsActive = false;
                    }
                    _logger.LogInformation($"Deactivated {expiredOffers.Count} expired offers.");
                    changesMade = true;
                }

                if (changesMade)
                {
                    await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database maintenance.");
            }
        }
    }
}
