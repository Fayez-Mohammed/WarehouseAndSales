using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Base.Shared.Enums;
using BaseAPI.Validation.SupplierValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SystemAdmin")]
public class SupplierController : ControllerBase
{
    private readonly IUnitOfWork _unit;
    private readonly SupplierPostValidation _postValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public SupplierController(
        IUnitOfWork unit,
        SupplierPostValidation postValidator,
        UserManager<ApplicationUser> userManager)
    {
        _unit = unit;
        _postValidator = postValidator;
        _userManager = userManager;
    }
    /// <summary>
    /// جلب جميع الموردين مع الترحيل للمعاملات الخاصة بهم
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        if (skip < 0 || take < 0 || take > 100)
            return BadRequest();

        try
        {
            var spec = new BaseSpecification<Supplier>();
            spec.ApplyPaging(skip, take);
            spec.Includes.Add(x => x.SupplyTransactions);

            var list = await _unit.Repository<Supplier>().ListAsync(spec);

            var response = list.Select(x => new SupplierReturnDto
            {
                SupplierId = x.Id,
                Name = x.Name,
                Address = x.Address,
               // ContactInfo = x.ContactInfo
            })
            .ToList();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Error getting suppliers" });
        }
    }

    /// <summary>
    /// إضافة مورد جديد مع إنشاء مستخدم مرتبط به
    /// </summary>
    /// <param name="supplierDto"></param>
    /// <returns></returns>
    [HttpPost("suppliers")]
    public async Task<IActionResult> AddSupplier([FromBody] SupplierPostDto supplierDto)
    {
        var validation = await _postValidator.ValidateAsync(supplierDto);

        if (!validation.IsValid)
            return BadRequest(new ApiResponseDTO { Message = "Invalid input parameters" });

        try
        {
           
            var user = new ApplicationUser
            {

                FullName = supplierDto.Name,
                UserName = supplierDto.Email,
                Email = supplierDto.Email,
                PhoneNumber = supplierDto.PhoneNumber,
                
            };
            user.Type = UserTypes.Supplier;
            var createUser = await _userManager.CreateAsync(user, supplierDto.Password);

            if (!createUser.Succeeded)
            {
                return BadRequest(new ApiResponseDTO
                {
                    Message = "Error creating user",
                    Data = createUser.Errors.Select(e => e.Description).ToList()
                });
            }

       var res=   await  _userManager.AddToRoleAsync(user,UserTypes.Supplier.ToString());
            if (!res.Succeeded)
            {
                return BadRequest(new ApiResponseDTO
                {
                    Message = "Error assigning role to user",
                    Data = res.Errors.Select(e => e.Description).ToList()
                });
            }
            var supplier = new Supplier
            {
                Name = supplierDto.Name,
             
                Address = supplierDto.Address,
                UserId = user.Id
            };

            await _unit.Repository<Supplier>().AddAsync(supplier);
            var completed = await _unit.CompleteAsync();

            if (completed <= 0)
                return BadRequest(new ApiResponseDTO { Message = "Error adding supplier" });

            return Created(nameof(AddSupplier), supplier);
        }
        catch
        {
            return StatusCode(500, new ApiResponseDTO { Message = "Server error while adding supplier" });
        }
    }
}










//using Base.API.DTOs;
//using Base.DAL.Models.SystemModels;
//using Base.Repo.Interfaces;
//using BaseAPI.Validation.SupplierValidation;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using RepositoryProject.Specifications;

//namespace Base.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
////[Authorize]
//public class SupplierController(IUnitOfWork unit, SupplierPostValidation postValidator) : ControllerBase
//{
//    [HttpGet("suppliers")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> GetSuppliers(
//        [FromQuery] int skip = 0
//        , [FromQuery] int take = 100)
//    {
//        if (skip < 0 || take < 0 || take > 100)
//            return BadRequest();
//        try
//        {
//            var spec = new BaseSpecification<Supplier>();
//            spec.ApplyPaging(skip, take);
//            spec.Includes.Add(x => x.SupplyTransactions);
//            var list = await unit.Repository<Supplier>()
//                .ListAsync(spec);

//            var response = list.Select(x => new SupplierReturnDto
//            {
//                SupplierId = x.Id,
//                Name = x.Name,
//                Address = x.Address,
//                ContactInfo = x.ContactInfo,
//            })
//            .ToList();
//            return Ok(response);
//            //   return Ok(new ApiResponseDTO {Data = response,Message = "Success"});
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new ApiResponseDTO { Message = "Error getting suppliers" });
//        }
//    }

//    [HttpPost("suppliers")]
//    [ProducesResponseType(StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> AddSupplier([FromBody] SupplierPostDto supplierDto)
//    {

//        var result = await postValidator.ValidateAsync(supplierDto);
//        if (!result.IsValid)
//            return BadRequest(new ApiResponseDTO { Message = "Invalid input parameters" });
//        try
//        {
//            Supplier supplier = new Supplier
//            {
//                Name = supplierDto.Name,
//                ContactInfo = supplierDto.ContactInfo,
//                Address = supplierDto.Address,
//            };
//            await unit.Repository<Supplier>().AddAsync(supplier);
//            var complete = await unit.CompleteAsync();
//            if (complete <= 0)
//                return BadRequest(new ApiResponseDTO { Message = "Error adding supplier" });

//            return Ok(complete);
//            //return Created();
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new ApiResponseDTO { Message = "Error adding supplier" });
//        }
//    }
//}