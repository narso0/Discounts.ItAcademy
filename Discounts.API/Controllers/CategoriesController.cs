// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Services;
using Discounts.Domain.Entities;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

public record CategoryDto(int Id, string Name);
public record CreateCategoryDto(string Name);

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name cannot be empty.")
            .MinimumLength(3).WithMessage("Category name must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters.");
    }
}
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
        var dtos = categories.Adapt<IEnumerable<CategoryDto>>();
        return Ok(dtos);
    }
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var validator = new CreateCategoryValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        return Ok(new { Message = "Category validated and ready!", Category = dto.Name });
    }
}
