// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class MerchantRepository :  IMerchantRepository
    {
        private readonly DiscountsDbContext _context;

        public MerchantRepository(DiscountsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Merchant>> GetPendingMerchantsAsync()
        {
            return await _context.Merchants
                .Where(m => (int)m.Status == 0)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }
        public async Task<Merchant?> GetByEmailAsync(string email) =>
            await _context.Merchants.FirstOrDefaultAsync(m => m.Email == email).ConfigureAwait(false);

        public async Task AddAsync(Merchant merchant)
        {
            _context.Merchants.Add(merchant);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
