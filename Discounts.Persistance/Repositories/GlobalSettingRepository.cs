// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories;

public class GlobalSettingRepository : IGlobalSettingRepository
{
    private readonly DiscountsDbContext _context;

    public GlobalSettingRepository(DiscountsDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalSetting> GetSettingsAsync()
    {
        var settings = await _context.GlobalSettings.FirstOrDefaultAsync().ConfigureAwait(false);

        if (settings == null)
        {
            settings = new GlobalSetting();
            _context.GlobalSettings.Add(settings);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(GlobalSetting settings)
    {
        _context.GlobalSettings.Update(settings);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
