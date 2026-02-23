// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Discounts.Web.Models.Admin
{
    public class SettingsViewModel
    {
        [Required]
        [Range(1, 1440, ErrorMessage = "Timeout must be between 1 minute and 24 hours (1440 mins).")]
        [Display(Name = "Reservation Timeout (Minutes)")]
        public int ReservationTimeoutMinutes { get; set; }

        [Required]
        [Range(1, 720, ErrorMessage = "Grace period must be between 1 and 720 hours (30 days).")]
        [Display(Name = "Merchant Edit Grace Period (Hours)")]
        public int MerchantEditGracePeriodHours { get; set; }
    }
}
