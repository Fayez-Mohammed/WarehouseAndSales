using Base.API.DTOs;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Base.Repo.Specifications;
using BaseAPI.Validation.ProductValidation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Accountant,SystemAdmin,StoreManager")]
public class InventoryController(IUnitOfWork unit
   , ProductDtoValidation productValidator
   , ProductUpdateDtoValidation productUpdateValidator
   , ILogger<Product> logger) : ControllerBase
{
    /// <summary>
    /// جلب جميع المنتجات مع التصفية والفرز
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    [HttpGet("GetAllproducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int take)
    {

        try
        {
            var spec = new BaseSpecification<Product>(p=>p.IsDeleted == false);
           // spec.Includes.Add(i => i.Products);
            spec.ApplyPaging(skip, take);


            var inventory = await unit
               .Repository<Product>()
               .ListAsync(spec);
            var list =
               inventory
                  .Select(x => new { ProductId=x.Id,ProductName = x.Name, SellPrice = x.SellPrice, CategoryId = x.CategoryId });

           
            return Ok(list);

            // return Ok(new ApiResponseDTO { Data = list, StatusCode = StatusCodes.Status200OK, Message = "OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while getting proudcts" });
        }
    }
    /// <summary>
    ///  جلب مواصفات منتج معين بواسطة المعرف
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductSpecifications([FromQuery] string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponseDTO { Message = "Invalid ID" });

        try
        {
            var product = await unit
               .Repository<Product>()
               .GetByIdAsync(id);
            if (string.IsNullOrEmpty(product.Id) || string.IsNullOrEmpty(product.Name))
                return NotFound();

            var response = new ProductReturnDto()
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SalePrice = product.SellPrice,
                BuyPrice = product.BuyPrice,
                Quantity = product.CurrentStockQuantity,
                SKU = product.SKU,
                Description = product.Description,
                CategoryId = product.CategoryId
            };
            return Ok(response);
            //  return Ok(new ApiResponseDTO { Data = response, StatusCode = StatusCodes.Status200OK, Message = "OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while getting products" });
        }
    }
    /// <summary>
    /// انشاء منتج جديد
    /// </summary>
    /// <param name="productCreateDto"></param>
    /// <returns></returns>
    [HttpPost("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct([FromQuery]string SupplierId,[FromBody] ProductDto productCreateDto)
    {
        var validate = await productValidator.ValidateAsync(productCreateDto);
        if (!validate.IsValid)
            return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
       
        try
        {
            var product = new Product()
            {
                
                Name = productCreateDto.ProductName,
                SKU = productCreateDto.SKU,
                SellPrice = productCreateDto.SalePrice,
                BuyPrice = productCreateDto.BuyPrice,
                Description = productCreateDto.Description,
                CurrentStockQuantity = productCreateDto.Quantity,
                
                CategoryId = productCreateDto.CategoryId
            };

            await unit.Repository<Product>().AddAsync(product);
            var result = await unit.CompleteAsync();
            if (result == 0)
                return BadRequest(new ApiResponseDTO { Message = "Error occured while adding product" });
           return Ok(result);
            // return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status201Created });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while creating product" });
        }
    }
    /// <summary>
    /// تحديث منتج موجود
    /// </summary>
    /// <param name="productDto"></param>
    /// <returns></returns>
    [HttpPut("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct( [FromBody] ProductUpdateDto productDto)
    {
        var validate = await productUpdateValidator.ValidateAsync(productDto);

        if (!validate.IsValid)
        {
            return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
        }

        try
        {
            var product = await unit.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (string.IsNullOrEmpty(product.Name))
                return NotFound();
            product.Name = productDto.ProductName;
            product.SKU = productDto.SKU;
            product.SellPrice = productDto.SellPrice;
            product.BuyPrice = productDto.BuyPrice;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId;

            var result = await unit.CompleteAsync();

            if (result == 0)
                return BadRequest(new ApiResponseDTO { Message = "Error occured while updating product" });
           return Ok(result);
            //return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status200OK });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while updating product" });
        }
    }
    /// <summary>
    /// حذف منتج بواسطة المعرف
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("products")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct([FromQuery] string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponseDTO { Message = "Invalid ID" });
        try
        {
            var product = await unit.Repository<Product>().GetByIdAsync(id);

            if (String.IsNullOrEmpty(product.Name))
                return NotFound();

            product.IsDeleted = true;

            var result = await unit.CompleteAsync();

            if (result == 0)
                return BadRequest(new ApiResponseDTO { Message = "Error occured while deleting product" });

            return Ok(result);
            //return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while deleting product" });
        }
    }
    /// <summary>
    /// تحديث كمية المخزون لمنتج معين (زيادة الكمية)
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="quantity"></param>
    /// <param name="supplierId"></param>
    /// <returns></returns>
    [HttpPost("products/stock/in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventoryQuantity([FromQuery] string productId, [FromQuery] int quantity, [FromQuery] string supplierId)
    {
        if (string.IsNullOrEmpty(productId))
            return BadRequest(new ApiResponseDTO { Message = "Invalid ID" });

        if (quantity <= 0)
            return BadRequest(new ApiResponseDTO { Message = "Invalid Quantity" });

        try
        {
            var product = await unit.Repository<Product>().GetByIdAsync(productId);

            if (string.IsNullOrEmpty(product.Name))
                return NotFound();

            product.SupplierId = supplierId;
            product.CurrentStockQuantity += quantity;

            var StockTransaction = new StockTransaction()
            {
                ProductId = productId,
                Quantity = quantity,
                Type = DAL.Models.SystemModels.Enums.TransactionType.StockIn,
                DateOfCreation = DateTime.UtcNow
            };

            await unit.Repository<StockTransaction>().AddAsync(StockTransaction);

            var result = await unit.CompleteAsync();

            if (result == 0)
              
                return BadRequest(new ApiResponseDTO { Message = "Error occured while updating product" });
           
            return Ok(result);
            // return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while updating inventory quantity" });
        }
    }
}