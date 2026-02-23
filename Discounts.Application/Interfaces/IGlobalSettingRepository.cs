// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entities;

namespace Discounts.Application.Interfaces
{
    public interface IGlobalSettingRepository
    {
        Task<GlobalSetting> GetSettingsAsync();
        Task UpdateSettingsAsync(GlobalSetting settings);
    }
}
