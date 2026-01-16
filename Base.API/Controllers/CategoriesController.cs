using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            var spec = new BaseSpecification<Category>(c => true && c.IsDeleted == false);
            spec.AddOrderBy(c => c.Name);
            var categories = await repo.ListAsync(spec);

            var result = categories.Select(c => new
            {
                c.Id,
                c.Code,
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
        [HttpGet("GetCategoryByName")]
        public async Task<IActionResult> GetCategoryByName([FromQuery] string name)
        {
            //   var category = await _unitOfWork.Repository<Category>().Get(name);
            var repo = _unitOfWork.Repository<Category>();
            var spec = new BaseSpecification<Category>(c => c.Name.Contains( name) && c.IsDeleted == false);
            var category = (await repo.ListAsync(spec)).FirstOrDefault();
            if (category == null)
                return NotFound($"Category with Name {name} not found.");

            return Ok(new
            {
                category.Id,
                category.Code,
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
            //var repoCat = _unitOfWork.Repository<Category>();
            //var specCat = new BaseSpecification<Category>(c => c.Name == dto.Name);
            //var OldCategory = repoCat.GetEntityWithSpecAsync(specCat);
            //if(OldCategory !=null&&OldCategory.)
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
            var repoProducts = _unitOfWork.Repository<Product>();
            var specProduct = new BaseSpecification<Product>(p => p.CategoryId == id);
            var products =await repoProducts.ListAsync(specProduct);
            if(products.Count>0)
                 return StatusCode(500, "There is products in this category , it is not allowed to remove categry has products.");
            // Soft delete
            category.IsDeleted = true;
            category.Name = category.Name + "_Deleted_" + category.Code;
            category.DateOfDeletion = DateTime.UtcNow;
            category.DeletedById= User.FindFirstValue(ClaimTypes.NameIdentifier);
            await repo.UpdateAsync(category);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                return StatusCode(500, "Failed to delete category.");
            return Ok(new { Message = "Category deleted successfully" });

        }



        /// <summary>
        /// (Autocomplete) إكمال تلقائي للبحث: يرجع قائمة مصغرة بالأسماء المقترحة
        /// </summary>
        [HttpGet("autocomplete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategorySuggestions([FromQuery] string term)
        {
            // لو مفيش كلمة بحث، نرجع قائمة فاضية
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<object>());

            try
            {
                var repo = _unitOfWork.Repository<Category>();

                // 1. المواصفات: البحث عن الاسم يحتوي على الكلمة + غير محذوف
                // استخدام Contains بيخلي البحث مرن (لو كتب "موبايل" يطلع "سامسونج موبايل")
                var spec = new BaseSpecification<Category>(p =>
                    p.Name.Contains(term) && !p.IsDeleted);

                // 2. الترتيب: أبجدياً عشان الشكل يكون منظم
                spec.AddOrderBy(p => p.Name);

                // 3. التحجيم (Paging): أهم خطوة! هات أول 10 نتايج بس عشان الـ Dropdown ميهنجش
                spec.ApplyPaging(0, 10);

                // تنفيذ الاستعلام
                var categories = await repo.ListAsync(spec);

                // 4. اختيار البيانات (Projection): رجع بس اللي الـ Frontend محتاجه (الاسم والـ ID)
                var suggestions = categories.Select(p => new
                {
                    p.Code,
                    p.Name,
                    // ممكن تزود السعر لو عايز تعرضه جنب الاسم
                    // Price = p.SellPrice 
                });

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error getting suggestions"); // لو عندك Logger
                return StatusCode(500, new { Message = "Error retrieving suggestions" });
            }
        }

    }
}