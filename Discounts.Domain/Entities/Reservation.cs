namespace Discounts.Domain.Entities;

public class Reservation : BaseEntity
{
    public string CustomerEmail { get; set;} = string.Empty;
    public string CouponCode { get; set;} = string.Empty;
    public DateTime ReservationTime { get; set;} = DateTime.UtcNow;
    public DateTime ExpiryTime { get; set;}
    public bool IsUsed{ get; set;} = false;
    public int DiscountId { get; set;}
    public Discount? Title { get; set;}
}