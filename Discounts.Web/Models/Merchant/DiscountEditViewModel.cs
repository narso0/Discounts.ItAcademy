// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discounts.Web.Models.Merchant
{
    public class DiscountEditViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Url]
        public string? ImageUrl { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal OriginalPrice { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal DiscountedPrice { get; set; }

        [Required, Range(1, 10000)]
        public int Quantity { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
