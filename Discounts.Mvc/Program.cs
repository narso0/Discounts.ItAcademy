using Discounts.Application.Interfaces;
using Discounts.Application.Services;
using Discounts.Infrastructure.Context;
using Discounts.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DiscountsDbContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DiscountsDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IGlobalSettingRepository, GlobalSettingRepository>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<Discounts.Application.Services.ICategoryService, Discounts.Application.Services.CategoryService>();
builder.Services.AddScoped<Discounts.Application.Services.IAdminService, Discounts.Application.Services.AdminService>();
builder.Services.AddScoped<Discounts.Application.Interfaces.IMerchantRepository, Discounts.Infrastructure.Repositories.MerchantRepository>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddControllersWithViews();
//N10 health check
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();
//middleware(s)
app.UseMiddleware<Discounts.Web.Middlewares.GlobalExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync().ConfigureAwait(false);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
