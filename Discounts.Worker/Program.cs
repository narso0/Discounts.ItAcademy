using Discounts.Infrastructure.Context;
using Discounts.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

//injecting db
builder.Services.AddDbContext<DiscountsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//registering worker class
builder.Services.AddHostedService<SystemMaintenanceWorker>();

var host = builder.Build();
host.Run();
