// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Context
{
    public class DbInitializer : IDbInitializer
    {
        private readonly DiscountsDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(DiscountsDbContext context, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            if ((await _context.Database.GetPendingMigrationsAsync().ConfigureAwait(false)).Any())
            {
                await _context.Database.MigrateAsync().ConfigureAwait(false);
            }

            if (!await _roleManager.RoleExistsAsync("Admin").ConfigureAwait(false))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin")).ConfigureAwait(false);
                await _roleManager.CreateAsync(new IdentityRole("Merchant")).ConfigureAwait(false);
                await _roleManager.CreateAsync(new IdentityRole("Customer")).ConfigureAwait(false);

                var adminUser = new IdentityUser { UserName = "admin@narso.com", Email = "admin@narso.com", EmailConfirmed = true };                await _userManager.CreateAsync(adminUser, "Admin123!").ConfigureAwait(false);
                await _userManager.AddToRoleAsync(adminUser, "Admin").ConfigureAwait(false);
            }

        }
    }
}
