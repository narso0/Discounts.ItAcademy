// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Web.Models.Home
{
    public class StoreFrontViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int QuantityAvailable { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
