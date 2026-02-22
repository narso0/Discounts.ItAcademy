// Copyright (C) TBC Bank. All Rights Reserved.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discounts.Web.Models.Merchant
{
    public class DiscountCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Original price must be greater than 0")]
        [Display(Name = "Original Price")]
        public decimal OriginalPrice { get; set; }
        [Required]
        [Range(0.01, 100000, ErrorMessage = "Discounted price must be greater than 0")]
        [Display(Name = "Discounted Price")]
        public decimal DiscountedPrice { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
