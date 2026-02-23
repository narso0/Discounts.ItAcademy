// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Common;

namespace Discounts.Domain.Entities
{
    public class GlobalSetting : BaseEntity
    {
        public int ReservationTimeoutMinutes { get; set; } = 30;
        public int MerchantEditGracePeriodHours { get; set; } = 24;
    }
}
