// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Web.Models.Merchant
{
    public class SalesHistoryViewModel
    {
        public string DiscountTitle { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
    }
}
