// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.Interfaces;
using Discounts.Domain.Entities;
using Discounts.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync().ConfigureAwait(false);
            var viewModels = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var category = new Category{Name = model.Name};
            await _categoryRepository.AddAsync(category).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);
            if(category == null) return NotFound();
            var model = new CategoryViewModel{Id = category.Id, Name = category.Name};
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, CategoryViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var category = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);
            if(category == null) return NotFound();
            category.Name = model.Name;
            await _categoryRepository.UpdateAsync(category).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);
            if(category == null) return NotFound();
            await _categoryRepository.DeleteAsync(category).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

    }
}
