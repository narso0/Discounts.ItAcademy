namespace Discounts.Domain.Entities;

public class Merchant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Number { get; set; }
    public string? Email { get; set; }
    public MerchantStatus Status { get; set; } = MerchantStatus.Pending;

    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}