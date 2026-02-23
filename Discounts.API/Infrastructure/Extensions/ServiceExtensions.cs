// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Infrastructure.Repositories;

namespace Discounts.API.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IGlobalSettingRepository, GlobalSettingRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();

        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IMerchantService, MerchantService>();

        return services;
    }
}
