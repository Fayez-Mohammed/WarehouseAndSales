using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Base.Shared.Enums;
using BaseAPI.Validation.SupplierValidation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SystemAdmin,Accountant")]
public class AccountantProfilesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
        private readonly AccountantPostDtoValidator _postValidator;
    private readonly UserManager<ApplicationUser> _userManager;
    public AccountantProfilesController(IUnitOfWork unitOfWork, AccountantPostDtoValidator postValidator,UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _postValidator = postValidator;
        _userManager = userManager;
    }

    /// <summary>
    /// اضافة محاسب جديد
    /// </summary>
    /// <param name="accountantDto"></param>
    /// <returns></returns>
    [HttpPost("AddAccountant")]

    public async Task<IActionResult> AddAccountant([FromBody] AccountantPostDto accountantDto)
    {
        var validation = await _postValidator.ValidateAsync(accountantDto);
        if (!validation.IsValid)
            return BadRequest(new ApiResponseDTO
            {
                Message = "Invalid input parameters",
                Data = validation.Errors.Select(e => e.ErrorMessage).ToList()
            });

        try
        {
            var user = new ApplicationUser
            {
                FullName = accountantDto.Name,
                UserName = accountantDto.Email,
                Email = accountantDto.Email,
                PhoneNumber = accountantDto.PhoneNumber,
                Type = UserTypes.Accountant
            };

            var createUser = await _userManager.CreateAsync(user, accountantDto.Password);
            if (!createUser.Succeeded)
            {
                return BadRequest(new ApiResponseDTO
                {
                    Message = "Error creating user",
                    Data = createUser.Errors.Select(e => e.Description).ToList()
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, UserTypes.Accountant.ToString());
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new ApiResponseDTO
                {
                    Message = "Error assigning role to user",
                    Data = roleResult.Errors.Select(e => e.Description).ToList()
                });
            }

            var profile = new AccountantUserProfile
            {
                UserId = user.Id
            };

            await _unitOfWork.Repository<AccountantUserProfile>().AddAsync(profile);
            var completed = await _unitOfWork.CompleteAsync();

            if (completed <= 0)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new ApiResponseDTO { Message = "Error adding accountant profile" });
            }

            return Created(nameof(AddAccountant), new
            {
                UserId = user.Id,
                ProfileId = profile.Id,
                Message = "Accountant created successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseDTO
            {
                Message = "Server error while adding accountant",
                Data = new[] { ex.Message }
            });
        }

    }
    /// <summary>
    /// جلب جميع المحاسبين
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetAllAccountants")]
        public async Task<IActionResult> GetAllAccountants()
        {
            var accountants = await _unitOfWork.Repository<AccountantUserProfile>().ListAllAsync();
           var  accountantDTOs= accountants.Select(a => new AccountantGetDto
            {
                Id = a.Id,
                UserId = a.UserId,
               Name = a.User.FullName,
                Email = a.User.Email,
                PhoneNumber = a.User.PhoneNumber
            }).ToList();
        return Ok(accountantDTOs);
        }

    /// <summary>
    /// جلب بيانات المحاسب الحالي
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetCurrentAccountant")]
    public IActionResult GetCurrentAccountant()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new ApiResponseDTO { Message = "User ID not found in token" });
        }
        var user = _userManager.FindByIdAsync(userId).Result;
        if (user == null)
        {
            return NotFound(new ApiResponseDTO { Message = "User not found" });
        }
        var accountantDto = new AccountantGetDto
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
        return Ok(accountantDto);
    }
}

