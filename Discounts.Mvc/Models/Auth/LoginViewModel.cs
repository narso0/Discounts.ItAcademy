// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Discounts.Web.Models.Auth
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
