using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]// Adjust roles as needed, e.g., [Authorize(Roles = "StoreManager")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var repo = _unitOfWork.Repository<Category>();
            // Use BaseSpecification to get all categories (no filter)
            // You can add .ApplyPaging(skip, take) here if you want pagination later
            var spec = new BaseSpecification<Category>(c => true);

            var categories = await repo.ListAsync(spec);

            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.DateOfCreation
            });

            return Ok(result);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Category details</returns>
        [HttpGet("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById([FromQuery] string id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);

            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            return Ok(new
            {
                category.Id,
                category.Name,
                category.Description,
                category.DateOfCreation
            });
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateCategory")]
      
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                return StatusCode(500, "Failed to create category.");

            return Ok(new { Message = "Category created successfully", CategoryId = category.Id });
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateCategory")]
      
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(dto.Id);

            if (category == null)
                return NotFound($"Category with ID {dto.Id} not found.");

            category.Name = dto.Name;
            category.Description = dto.Description;

            await repo.UpdateAsync(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                return StatusCode(500, "Failed to update category.");

            return Ok(new { Message = "Category updated successfully" });
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCategory")]
  
        public async Task<IActionResult> DeleteCategory([FromQuery] string id)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(id);

            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            // Optional: Check if products are linked before deleting
            // var productRepo = _unitOfWork.Repository<Product>();
            // var spec = new BaseSpecification<Product>(p => p.CategoryId == id);
            // var count = await productRepo.CountAsync(spec);
            // if (count > 0) return BadRequest("Cannot delete category with associated products.");

            await repo.DeleteAsync(category);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                return StatusCode(500, "Failed to delete category.");

            return Ok(new { Message = "Category deleted successfully" });
        }
    }

 
}