// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DiscountsDbContext _context;
        public CategoryRepository(DiscountsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }
        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(new object[]{id}).ConfigureAwait(false);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
