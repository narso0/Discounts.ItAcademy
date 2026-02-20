using Discounts.Domain.Common;

namespace Discounts.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}
