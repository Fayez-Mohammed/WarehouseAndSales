using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Implementations;
using Base.Repo.Interfaces;
using Base.Repo.Specifications;
using Base.Services.Interfaces;
using Base.Shared.Enums;
using BaseAPI.Validation.ProductValidation;
using FluentValidation;
using Hangfire.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RepositoryProject.Specifications;
using System.Security.Claims;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]
public class InventoryController(IUnitOfWork unit
   , ProductDtoValidation productValidator
   , ProductUpdateDtoValidation productUpdateValidator
   , ILogger<Product> logger,IUserService userService, ProductWithCategoryNameDtoValidation productWithCategoryNameValidator,ProductUpdateWithCategoryNameDtoValidation productUpdateWithCategoryNameValidator) : ControllerBase
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
    public async Task<IActionResult> GetAll([FromQuery] int skip=0, [FromQuery] int take=10)
    {

        try
        {
            var spec = new BaseSpecification<Product>(p=>p.IsDeleted == false);
           // spec.Includes.Add(i => i.Products);
            spec.ApplyPaging(skip, take);
            spec.Includes.Add(i => i.Category);
            spec.AddOrderBy(p => p.Name);
            var inventory = await unit
               .Repository<Product>()
               .ListAsync(spec);
            
            //var list =
            //   inventory
            //      .Select(x => new { ProductId=x.Id,ProductName = x.Name, SellPrice = x.SellPrice, CurrentStockQuantity=x.CurrentStockQuantity/*,CategoryName=x.Category.Name*/ ,CategoryId = x.CategoryId });

           var productsDto= inventory
               .Select(x => new ProductReturnDto
               {
                   ProductId = x.Id,
                   ProductName = x.Name,
                   SalePrice = x.SellPrice,
                   BuyPrice = x.BuyPrice,
                   Quantity = x.CurrentStockQuantity,
                   SKU = x.SKU,
                   Description = x.Description,
                   CategoryId = x.CategoryId,
                   CategoryName=x.Category!=null?x.Category.Name:null
               });
            return Ok(productsDto);

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
    public async Task<IActionResult> GetProductSpecifications([FromQuery] string ProductName)
    {
        if (string.IsNullOrEmpty(ProductName))
            return BadRequest(new ApiResponseDTO { Message = "Invalid Product Name" });

        try
        {
            //var product = await unit
            //   .Repository<Product>()
            //   .GetByIdAsync(id);

            var repo = unit.Repository<Product>();
            var spec = new BaseSpecification<Product>(p => p.Name.Contains(ProductName) && p.IsDeleted == false);
            spec.Includes.Add(i => i.Category);

            var product = await repo.ListAsync(spec);

            if (product == null || !product.Any())
                return NotFound();

            var response = product.Select(p => new ProductReturnDto()
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SalePrice = p.SellPrice,
                BuyPrice = p.BuyPrice,
                Quantity = p.CurrentStockQuantity,
                SKU = p.SKU,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null
            });
            return Ok(response);
            //  return Ok(new ApiResponseDTO { Data = response, StatusCode = StatusCodes.Status200OK, Message = "OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while getting products" });
        }
    }
    ///// <summary>
    ///// انشاء منتج جديد
    ///// </summary>
    ///// <param name="productCreateDto"></param>
    ///// <returns></returns>
    //[HttpPost("products")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> CreateProduct([FromQuery]string SupplierId,[FromBody] ProductDto productCreateDto)
    //{

    //    var validate = await productValidator.ValidateAsync(productCreateDto);
    //    if (!validate.IsValid)
    //        return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
    //    var repo = unit.Repository<Supplier>();
    //    var spec = new BaseSpecification<Supplier>(s => s.UserId == SupplierId);
    //    var supplier = await repo.GetEntityWithSpecAsync(spec);
    //    decimal totalPrice = 0;
    //    try
    //    {
    //        var repo2 = unit.Repository<Category>();
    //        var spec2 = new BaseSpecification<Category>(c => c.Name == productCreateDto.CategoryName && c.IsDeleted == false);
    //        var category = (await repo2.ListAsync(spec2)).FirstOrDefault();
    //        if (category == null)
    //        {
    //            return BadRequest(new ApiResponseDTO { Message = $"Category '{productCreateDto.CategoryName}' not found." });
    //        }
    //        var product = new Product()
    //        {
                
    //            Name = productCreateDto.ProductName,
    //            SKU = productCreateDto.SKU,
    //            SellPrice = productCreateDto.SalePrice,
    //            BuyPrice = productCreateDto.BuyPrice,
    //            Description = productCreateDto.Description,
    //            CurrentStockQuantity = productCreateDto.Quantity,
                
    //            CategoryId = category.Id
    //        };

    //        await unit.Repository<Product>().AddAsync(product);
    //        totalPrice += (product.BuyPrice * product.CurrentStockQuantity);
    //        // Create Stock Transaction Log
    //        var stockLog = new StockTransaction
    //        {
    //            ProductId = product.Id,
    //            SupplierId = supplier.Id,
    //            Type = TransactionType.StockIn,
    //            Quantity = product.CurrentStockQuantity,
    //            DateOfCreation = DateTime.UtcNow,
    //            UnitBuyPrice = product.BuyPrice,
    //            UnitSellPrice = product.SellPrice,
    //            Notes = "Updated via Single product addition"

    //            // REMOVED: TransactionDate = DateTime.UtcNow 
    //            // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
    //        };

    //        await unit.Repository<StockTransaction>().AddAsync(stockLog);
    //        // 3. Generate Supplier Invoice
    //        var supplierInvoice = new SupplierInvoice
    //        {
    //            SupplierId = supplier.Id,
    //            Type = InvoiceType.SupplierInvoice,
    //            SupplierName = supplier.Name,
    //            Amount = totalPrice,
    //            RemainingAmount = totalPrice,
    //            DateOfCreation = DateTime.UtcNow
    //        };
    //        await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);
    //        var result = await unit.CompleteAsync();
    //        if (result == 0)
    //            return BadRequest(new ApiResponseDTO { Message = "Error occured while adding product" });
    //       return Ok(result);
    //        // return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status201Created });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponseDTO { Message = "Error while creating product" });
    //    }
    //}

//    /// <summary>
//    /// ادخال مجموعه من المنتجات اللي جابها المورد
//    /// </summary>
//    /// <param name="SupplierId"></param>
//    /// <param name="productCreateDto"></param>
//    /// <returns></returns>
    
//    [HttpPost("ListOfproducts")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> CreateListOfProducts([FromQuery] string supplierName, [FromBody] List<ProductDto> productCreateDto)
//    {
//        //  ApplicationUser user =await userService.GetByIdAsync(SupplierId);
//        var repo = unit.Repository<Supplier>();
//        //var spec = new BaseSpecification<Supplier>(s => s.UserId == SupplierId);
//        //var supplier = await repo.GetEntityWithSpecAsync(spec);

//        var spec = new BaseSpecification<Supplier>(
//    s => s.Name.Contains( supplierName)
//);

//        var suppliers = await repo.ListAsync(spec);

//        if (!suppliers.Any())
//            throw new Exception("Supplier not found");

//        if (suppliers.Count > 1)
//            throw new Exception("Multiple suppliers found with the same name");

//        var supplier = suppliers.First();

       
//        decimal totalPrice = 0;
//        foreach (var productDto in productCreateDto)
//        {
//            var validate = await productValidator.ValidateAsync(productDto);
//            if (!validate.IsValid)
//                return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });

//            var repo2 = unit.Repository<Category>();
//            var spec2 = new BaseSpecification<Category>(c => c.Name ==productDto.CategoryName  && c.IsDeleted == false);
//            var category = (await repo2.ListAsync(spec2)).FirstOrDefault();
//            if (category == null)
//                {
//                return BadRequest(new ApiResponseDTO { Message = $"Category '{productDto.CategoryName}' not found." });
//            }
//            var product = new Product()
//            {
//                Name = productDto.ProductName,
//                SKU = productDto.SKU,
//                SellPrice = productDto.SalePrice,
//                BuyPrice = productDto.BuyPrice,
//                Description = productDto.Description,
//                CurrentStockQuantity = productDto.Quantity,
//                CategoryId = category.Id

//            };
//            totalPrice += (productDto.BuyPrice*productDto.Quantity);
//            await unit.Repository<Product>().AddAsync(product);

//            // Create Stock Transaction Log
//            var stockLog = new StockTransaction
//            {
//                ProductId = product.Id,
//                SupplierId = supplier.Id,
//                Type = TransactionType.StockIn,
//                Quantity = product.CurrentStockQuantity,
//                DateOfCreation = DateTime.UtcNow,
//                UnitBuyPrice=product.BuyPrice,
//                UnitSellPrice=product.SellPrice,
//                Notes= "Updated via bulk product addition"

//                // REMOVED: TransactionDate = DateTime.UtcNow 
//                // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
//            };
//            await unit.Repository<StockTransaction>().AddAsync(stockLog);

//        }
//        try
//        {
          
            
//            // 3. Generate Supplier Invoice
//            var supplierInvoice = new SupplierInvoice
//            {
//                SupplierId = supplier.Id,
//                Type = InvoiceType.SupplierInvoice,
//                SupplierName = supplier.Name,
//                Amount = totalPrice,
//                RemainingAmount = totalPrice,
//                DateOfCreation = DateTime.UtcNow
//            };
//            await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);
//            var result = await unit.CompleteAsync();
//            if (result == 0)
//                return BadRequest(new ApiResponseDTO { Message = "Error occured while adding products" });
//            return Ok(result);
//            // return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status201Created });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new ApiResponseDTO { Message = "Error while creating products" });
//        }
//    }

    /// <summary>
    /// اضافة مجموعه من المنتجات مع اسم التصنيف
    /// </summary>
    /// <param name="supplierName"></param>
    /// <param name="productCreateWithNameDto"></param>
    /// <returns></returns>
    [HttpPost("ListOfProductsWithCategoryName")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateListOfProducts([FromQuery] string supplierName, [FromBody] List<ProductWithCategoryNameDto> productCreateWithNameDto)
    {
        var repoProducts = unit.Repository<Product>();

        //  ApplicationUser user =await userService.GetByIdAsync(SupplierId);
        var repo = unit.Repository<Supplier>();
        //var spec = new BaseSpecification<Supplier>(s => s.UserId == SupplierId);
        //var supplier = await repo.GetEntityWithSpecAsync(spec);
        var spec = new BaseSpecification<Supplier>(
s => s.Name.Contains(supplierName)
);

        var suppliers = await repo.ListAsync(spec);

        if (!suppliers.Any())
            throw new Exception("Supplier not found");

        if (suppliers.Count > 1)
            throw new Exception("Multiple suppliers found with the same name");

        var supplier = suppliers.First();
        decimal totalPrice = 0;
        foreach (var productDto in productCreateWithNameDto)
        {
            var repoCategory = unit.Repository<Category>();
            var specCategory = new BaseSpecification<Category>(c => c.Name == productDto.CategoryName);
            var category = await repoCategory.GetEntityWithSpecAsync(specCategory);
            var validate = await productWithCategoryNameValidator.ValidateAsync(productDto);
            if (!validate.IsValid)
                return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
            if(category == null)
            {
                return BadRequest(new ApiResponseDTO { Message = $"Category '{productDto.CategoryName}' not found." });
            }
            var product = new Product()
            {
                Name = productDto.ProductName,
                SKU = productDto.SKU,
                SellPrice = productDto.SalePrice,
                BuyPrice = productDto.BuyPrice,
                Description = productDto.Description,
                CurrentStockQuantity = productDto.Quantity,
                CategoryId = category.Id

            };
            totalPrice += (productDto.BuyPrice * productDto.Quantity);

            var specProduct =new BaseSpecification<Product>(p=>p.Name == productDto.ProductName);
            var OldProduct= await repoProducts.GetEntityWithSpecAsync(specProduct);
            if (OldProduct != null && OldProduct.IsDeleted == true)
            {
                OldProduct.IsDeleted = false;
                OldProduct.SKU = productDto.SKU ?? OldProduct.SKU;
                OldProduct.SellPrice = productDto.SalePrice;
                OldProduct.BuyPrice = productDto.BuyPrice;
                OldProduct.Description = productDto.Description ?? OldProduct.Description;
                OldProduct.CurrentStockQuantity = productDto.Quantity;
                OldProduct.CategoryId = category.Id ?? OldProduct.CategoryId;
              await  repoProducts.UpdateAsync(OldProduct);
                var stockLog = new StockTransaction
                {
                    ProductId = OldProduct.Id,
                    SupplierId = supplier.Id,
                    Type = TransactionType.StockIn,
                    Quantity = OldProduct.CurrentStockQuantity,
                    DateOfCreation = DateTime.UtcNow,
                    UnitBuyPrice = OldProduct.BuyPrice,
                    UnitSellPrice = OldProduct.SellPrice,
                    Notes = "Updated via bulk product addition [The Product Was Deleted and restored]"

                    // REMOVED: TransactionDate = DateTime.UtcNow 
                    // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
                };
                await unit.Repository<StockTransaction>().AddAsync(stockLog);
            }
            else
            {
                await unit.Repository<Product>().AddAsync(product);

                // Create Stock Transaction Log
                var stockLog = new StockTransaction
                {
                    ProductId = product.Id,
                    SupplierId = supplier.Id,
                    Type = TransactionType.StockIn,
                    Quantity = product.CurrentStockQuantity,
                    DateOfCreation = DateTime.UtcNow,
                    UnitBuyPrice = product.BuyPrice,
                    UnitSellPrice = product.SellPrice,
                    Notes = "Updated via bulk product addition"

                    // REMOVED: TransactionDate = DateTime.UtcNow 
                    // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
                };
                await unit.Repository<StockTransaction>().AddAsync(stockLog);
            }


        }
        try
        {


            // 3. Generate Supplier Invoice
            var supplierInvoice = new SupplierInvoice
            {
                SupplierId = supplier.Id,
                Type = InvoiceType.SupplierInvoice,
                SupplierName = supplier.Name,
                Amount = totalPrice,
                RemainingAmount = totalPrice,
                DateOfCreation = DateTime.UtcNow
            };
            await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);
            var result = await unit.CompleteAsync();
            if (result == 0)
                return BadRequest(new ApiResponseDTO { Message = "Error occured while adding products" });
            return Ok(result);
            // return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status201Created });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while creating products" });
        }
    }
    ///// <summary>
    ///// تحديث منتج موجود
    ///// </summary>
    ///// <param name="productDto"></param>
    ///// <returns></returns>
    //[HttpPut("products")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> UpdateProduct( [FromBody] ProductUpdateDto productDto)
    //{
    //    var validate = await productUpdateValidator.ValidateAsync(productDto);

    //    if (!validate.IsValid)
    //    {
    //        return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
    //    }

    //    try
    //    {
    //        var product = await unit.Repository<Product>().GetByIdAsync(productDto.ProductId);
    //        if (string.IsNullOrEmpty(product.Name))
    //            return NotFound();
    //        product.Name = productDto.ProductName??product.Name;
    //        product.SKU = productDto.SKU??product.SKU;
    //        if(productDto.SellPrice!=0)
    //        product.SellPrice = productDto.SellPrice;
    //        if(productDto.BuyPrice!=0)
    //            product.BuyPrice = productDto.BuyPrice;
    //        product.Description = productDto.Description;
    //        var repo2 = unit.Repository<Category>();
    //        var spec2 = new BaseSpecification<Category>(c => c.Name == productDto.CategoryName && c.IsDeleted == false);
    //        var category = (await repo2.ListAsync(spec2)).FirstOrDefault();

    //        product.CategoryId = category?.Id??product.CategoryId;

    //        var result = await unit.CompleteAsync();

    //        if (result == 0)
    //            return BadRequest(new ApiResponseDTO { Message = "Error occured while updating product" });
    //       return Ok(result);
    //        //return Ok(new ApiResponseDTO { Data = result, StatusCode = StatusCodes.Status200OK });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponseDTO { Message = "Error while updating product" });
    //    }
    //}
    /// <summary>
    ///   تحديث منتج موجودمع اسم التصنيف
    /// </summary>
    /// <param name="productDto"></param>
    /// <returns></returns>
    [HttpPut("productsWithCategoryName")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateWithCategoryNameDto productDto)
    {
        //var validate = await productUpdateWithCategoryNameValidator.ValidateAsync(productDto);

        //if (!validate.IsValid)
        //{
        //    return BadRequest(new ApiResponseDTO { Message = "Invalid Input parameters" });
        //}

        try
        {
            var repoCategory = unit.Repository<Category>();
            var specCategory = new BaseSpecification<Category>(c => c.Name == productDto.CategoryName);
            var category = await repoCategory.GetEntityWithSpecAsync(specCategory);
            var product = await unit.Repository<Product>().GetByIdAsync(productDto.ProductId);
          
            if(!productDto.ProductName.IsNullOrEmpty())
            product.Name = productDto.ProductName ?? product.Name;
            if(!productDto.SKU.IsNullOrEmpty())
            product.SKU = productDto.SKU ?? product.SKU;
            if (productDto.SellPrice != 0m && productDto.SellPrice.HasValue)
                product.SellPrice = productDto.SellPrice ?? product.SellPrice;
            //if (productDto.BuyPrice != 0)
            //    product.BuyPrice = productDto.BuyPrice;
             if(!productDto.Description.IsNullOrEmpty())
            product.Description = productDto.Description ?? product.Description;
             if(category!=null)
            product.CategoryId = category.Id ?? product.CategoryId;
            if (product.CurrentStockQuantity != productDto.Quantity && productDto.Quantity != 0 && productDto.Quantity.HasValue)
            {
                TransactionType transactionType;
                int quantityDifference = productDto.Quantity.Value - product.CurrentStockQuantity;
                if (quantityDifference > 0)
                {
                    transactionType = TransactionType.UpdatedInByEmployee;
                }
                else
                {
                    transactionType = TransactionType.UpdatedOutByEmployee;
                    //   quantityDifference = Math.Abs(quantityDifference);
                }
                // Create Stock Transaction Log
                var stockLog = new StockTransaction
                {
                    ProductId = product.Id,
                  //  SupplierId = product.SupplierId,
                    Type = transactionType,
                    Quantity = quantityDifference,
                    DateOfCreation = DateTime.UtcNow,
                    UnitBuyPrice = product.BuyPrice,
                    UnitSellPrice = product.SellPrice,
                    Notes = $"Updated By Employee with Id {User.FindFirstValue(ClaimTypes.NameIdentifier)}"
                    // REMOVED: TransactionDate = DateTime.UtcNow 
                    // Your AppDbContext automatically fills 'DateOfCreation' which serves as the date.
                };
                await unit.Repository<StockTransaction>().AddAsync(stockLog);

                product.CurrentStockQuantity = productDto.Quantity.Value;

            }
          await  unit.Repository<Product>().UpdateAsync(product);
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
    ///// <summary>
    ///// تحديث كمية المخزون لمنتج معين (زيادة الكمية)
    ///// </summary>
    ///// <param name="productId"></param>
    ///// <param name="quantity"></param>
    ///// <param name="supplierId"></param>
    ///// <returns></returns>
    //[HttpPost("products/stock/in")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> UpdateInventoryQuantity([FromQuery] string productId, [FromQuery] int quantity, [FromQuery] string supplierId)
    //{
    //    var repo = unit.Repository<Supplier>();
    //    var spec = new BaseSpecification<Supplier>(s => s.UserId == supplierId);
    //    var supplier = await repo.GetEntityWithSpecAsync(spec);
    //    decimal totalPrice = 0;
    //    if (string.IsNullOrEmpty(productId))
    //        return BadRequest(new ApiResponseDTO { Message = "Invalid ID" });

    //    if (quantity <= 0)
    //        return BadRequest(new ApiResponseDTO { Message = "Invalid Quantity" });

    //    try
    //    {
    //        var product = await unit.Repository<Product>().GetByIdAsync(productId);

    //        if (string.IsNullOrEmpty(product.Name))
    //            return NotFound();

    //        product.SupplierId = supplierId;
    //        product.CurrentStockQuantity += quantity;

    //        var StockTransaction = new StockTransaction()
    //        {
    //            ProductId = productId,
    //            SupplierId=supplierId,
    //            Quantity = quantity,
    //            Type = DAL.Models.SystemModels.Enums.TransactionType.StockIn,
    //            DateOfCreation = DateTime.UtcNow,
    //            UnitBuyPrice=product.BuyPrice,
    //            UnitSellPrice=product.SellPrice,
    //            Notes= "Updated via single product stock in"
    //        };
    //        totalPrice += (product.BuyPrice * quantity);
    //        await unit.Repository<StockTransaction>().AddAsync(StockTransaction);


    //        // 3. Generate Supplier Invoice
    //        var supplierInvoice = new SupplierInvoice
    //        {
    //            SupplierId = supplier.Id,
    //            Type = InvoiceType.SupplierInvoice,
    //            SupplierName = supplier.Name,
    //            Amount = totalPrice,
    //            RemainingAmount = totalPrice,
    //            DateOfCreation = DateTime.UtcNow
    //        };
    //        await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);
    //        var result = await unit.CompleteAsync();

    //        if (result == 0)

    //            return BadRequest(new ApiResponseDTO { Message = "Error occured while updating product" });

    //        return Ok(result);
    //        // return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponseDTO { Message = "Error while updating inventory quantity" });
    //    }

    //}


    /// <summary>
    /// تحديث كمية المخزون لمنتج معين (زيادة الكمية)
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="quantity"></param>
    /// <param name="supplierName"></param>
    /// <returns></returns>
    [HttpPost("products/stock/in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventoryQuantity([FromQuery] string productName, [FromQuery] int quantity, [FromQuery] string supplierName)
    {
        var repo = unit.Repository<Supplier>();
        var spec = new BaseSpecification<Supplier>(s => s.Name.Contains(supplierName));
        spec.Includes.Add(i => i.User);
        spec.Criteria = s => s.User.Type == UserTypes.Supplier;
        var supplier = await repo.GetEntityWithSpecAsync(spec);
        decimal totalPrice = 0;
        if (string.IsNullOrEmpty(productName))
            return BadRequest(new ApiResponseDTO { Message = "Invalid Product Name" });

        if (quantity <= 0)
            return BadRequest(new ApiResponseDTO { Message = "Invalid Quantity" });

        try
        {
            var product = await unit.Repository<Product>().GetEntityWithSpecAsync(new BaseSpecification<Product>(p => p.Name == productName));

            if (product == null)
                return NotFound();

            product.SupplierId = supplier.Id;
            product.CurrentStockQuantity += quantity;

            var StockTransaction = new StockTransaction()
            {
                ProductId = product.Id,
                SupplierId = supplier.User.Id,
                Quantity = quantity,
                Type = DAL.Models.SystemModels.Enums.TransactionType.StockIn,
                DateOfCreation = DateTime.UtcNow,
                UnitBuyPrice = product.BuyPrice,
                UnitSellPrice = product.SellPrice,
                Notes = "Updated via single product stock in"
            };
            totalPrice += (product.BuyPrice * quantity);
            await unit.Repository<StockTransaction>().AddAsync(StockTransaction);


            // 3. Generate Supplier Invoice
            var supplierInvoice = new SupplierInvoice
            {
                SupplierId = supplier.Id,
                Type = InvoiceType.SupplierInvoice,
                SupplierName = supplier.Name,
                Amount = totalPrice,
                RemainingAmount = totalPrice,
                DateOfCreation = DateTime.UtcNow
            };
            await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);
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
    /// <summary>
    /// تحديث كمية المخزون لمجموعة من المنتجات (زيادة الكمية)
    /// </summary>
    /// <param name="products"></param>
    /// <param name="supplierName"></param>
    /// <returns></returns>
    [HttpPost("Listproducts/stock/in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListInventoryQuantity([FromBody] List<ProductForUpdateDto> products, [FromQuery] string supplierName)
    {
        var repo = unit.Repository<Supplier>();
        var spec = new BaseSpecification<Supplier>(s => s.Name.Contains(supplierName));
        spec.Includes.Add(i => i.User);
        spec.Criteria = s => s.User.Type == UserTypes.Supplier;
        var supplier = await repo.GetEntityWithSpecAsync(spec);
        if (supplier == null)
        {
            return BadRequest(new ApiResponseDTO { Message = $"Supplier with Name '{supplierName}' does not exist." });
        }
        decimal totalPrice = 0;
        try
        {
            foreach (var item in products)
            {
                if (string.IsNullOrEmpty(item.ProductName))
                    return BadRequest(new ApiResponseDTO { Message = "Invalid Name" });
                if (item.Quantity <= 0)
                    return BadRequest(new ApiResponseDTO { Message = "Invalid Quantity" });
                var product = await unit.Repository<Product>().GetEntityWithSpecAsync(new BaseSpecification<Product>(p => p.Name == item.ProductName));
                if (product == null)
                    return NotFound();
                product.SupplierId = supplier.Id;
                product.CurrentStockQuantity += item.Quantity;

                var StockTransaction = new StockTransaction()
                {
                    ProductId = product.Id,
                    SupplierId = supplier.Id,
                    Quantity = item.Quantity,
                    Type = DAL.Models.SystemModels.Enums.TransactionType.StockIn,
                    DateOfCreation = DateTime.UtcNow,
                    UnitBuyPrice=product.BuyPrice,
                    UnitSellPrice=product.SellPrice,
                    Notes= "Updated via bulk product stock in"
                };
                totalPrice += (product.BuyPrice * item.Quantity);
                await unit.Repository<StockTransaction>().AddAsync(StockTransaction);
                await unit.Repository<Product>().UpdateAsync(product);
            }

            // 3. Generate Supplier Invoice
            var supplierInvoice = new SupplierInvoice
            {
                SupplierId = supplier.Id,
                Type = InvoiceType.SupplierInvoice,
                SupplierName = supplier.Name,
                Amount = totalPrice,
                RemainingAmount = totalPrice,
                DateOfCreation = DateTime.UtcNow
            };
            await unit.Repository<SupplierInvoice>().AddAsync(supplierInvoice);

            var result = await unit.CompleteAsync();
            if (result == 0)
                return BadRequest(new ApiResponseDTO { Message = "Error occured while updating products" });
            return Ok(result);
            // return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error while updating inventory quantity" });
        }
    }
}