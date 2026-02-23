// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;

namespace Discounts.Web.Models.Admin
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category Name must be between 3 and 50 characters.")]
        public string Name { get; set; } = null!;
    }
}
