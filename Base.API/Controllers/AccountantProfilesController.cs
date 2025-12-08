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

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SystemAdmin")]
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

}