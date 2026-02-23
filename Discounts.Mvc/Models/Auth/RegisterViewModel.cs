// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Discounts.Web.Models.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Role { get; set; }

        public string? MerchantName { get; set; }
        public string? MerchantDetails { get; set; }
        public string? MerchantPhone { get; set; }
    }
}
