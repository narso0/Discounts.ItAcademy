// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Web.Models.Merchant
{
    public class DiscountViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        //new
        public string? RejectionReason { get; set; }
    }
}
