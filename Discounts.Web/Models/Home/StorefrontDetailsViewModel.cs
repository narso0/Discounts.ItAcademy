// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Web.Models.Home
{
    public class StorefrontDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int QuantityAvailable { get; set; }
        public DateTime EndDate { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
