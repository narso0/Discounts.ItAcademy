// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly DiscountsDbContext _context;
        public DiscountRepository(DiscountsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllByMerchantAsync(int merchantId)
        {
            return await _context.Discounts
                .Where(d => d.MerchantId == merchantId)
                .Include(d => d.Category)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Discount?> GetByIdAsync(int id)
        {
            return await _context.Discounts
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id)
                .ConfigureAwait(false);
        }
        public async Task AddAsync(Discount discount)
        {
            await _context.Discounts.AddAsync(discount).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Discount discount)
        {
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
