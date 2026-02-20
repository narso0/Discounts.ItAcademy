namespace Discounts.Domain.Entities;

public class Discount : BaseEntity
{
    public string Title { get; set;} = string.Empty;
    public string Description { get; set;} = string.Empty;
    public string? ImageUrl { get; set;}
    public decimal OriginalPrice { get; set;}
    public decimal DiscountedPrice { get; set;}
    public int Quantity { get; set;}
    public DateTime StartDate { get; set;}
    public DateTime EndDate { get; set;}
    public bool IsActive { get; set;} = false;

    public int MerchantId { get; set;}
    public Merchant? Merchant { get; set;}
    public int CategoryId { get; set;}
    public Category? Category { get; set;}

    public ICollection<Reservation> Reservations = new List<Reservation>();
}