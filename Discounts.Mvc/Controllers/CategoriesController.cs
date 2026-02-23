using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Discounts.Application.Services;
using Discounts.Domain.Entities;

namespace Discounts.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync().ConfigureAwait(false);
        var viewModels = categories.Select(c => new Discounts.Web.Models.Admin.CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name
        });

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        // Hand the view an empty ViewModel
        return View(new Discounts.Web.Models.Admin.CategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Discounts.Web.Models.Admin.CategoryViewModel model) // Accept ViewModel
    {
        if (ModelState.IsValid)
        {
            // Map the ViewModel back into a pure Domain Entity so the Service can save it
            var category = new Category { Name = model.Name };

            await _categoryService.CreateCategoryAsync(category).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id).ConfigureAwait(false);
        if (category == null) return NotFound();
        var viewModel = new Discounts.Web.Models.Admin.CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Discounts.Web.Models.Admin.CategoryViewModel model) // Accept ViewModel
    {
        if (ModelState.IsValid)
        {
            var category = new Category
            {
                Id = model.Id,
                Name = model.Name
            };

            await _categoryService.UpdateCategoryAsync(category).ConfigureAwait(false);
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteCategoryAsync(id).ConfigureAwait(false);
        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
