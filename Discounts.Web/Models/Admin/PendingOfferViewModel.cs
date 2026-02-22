// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Web.Models.Admin
{
    public class PendingOfferViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string MerchantName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
