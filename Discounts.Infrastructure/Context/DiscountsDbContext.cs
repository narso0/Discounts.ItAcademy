using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Discounts.Domain.Entities;

namespace Discounts.Infrastructure.Context;

public class DiscountsDbContext : IdentityDbContext<IdentityUser>
{
    public DiscountsDbContext(DbContextOptions<DiscountsDbContext> options) : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<GlobalSetting> GlobalSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Discount>()
        .Property(p => p.OriginalPrice)
        .HasPrecision(18,2);
        builder.Entity<Discount>()
        .Property(p => p.DiscountedPrice)
        .HasPrecision(18,2);
        builder.Entity<Reservation>()
        .HasOne(x => x.Discount)
        .WithMany(x => x.Reservations)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
