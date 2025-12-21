using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Security.Claims;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]
    public class ExpensesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpensesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        /// <summary>
        /// إنشاء مصروف جديد
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
    
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

           // string accountantUserId = dto.AccountantUserId ?? currentUserId;
            string accountantUserId =  currentUserId;

          

            var expense = new Expense
            {
                Amount = dto.Amount,
                Description = dto.Description,
                AccountantUserId = accountantUserId,
                DateOfCreation = DateTime.UtcNow

            };

            await _unitOfWork.Repository<Expense>().AddAsync(expense);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                return StatusCode(500, "فشل في حفظ المصروف.");

            var accountant = await _userManager.FindByIdAsync(accountantUserId);

            var response = new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Description = expense.Description,
                CreatedAt = expense.DateOfCreation,
                AccountantUserId = accountantUserId,
                AccountantName = accountant?.UserName ?? "غير معروف"
            };

            return Ok(new { Message = "تم إضافة المصروف بنجاح", Expense = response });
        }

        /// <summary>
        /// جلب جميع المصروفات
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]

        public async Task<IActionResult> GetAllExpenses()
        {
            var spec = new BaseSpecification<Expense>();
            spec.Includes.Add(e => e.AccountantUser);
        

            var expenses = await _unitOfWork.Repository<Expense>().ListAsync(spec);

            var result = expenses.Select(e => new ExpenseResponseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Description = e.Description,
                //CreatedAt = e.CreatedAt,
                AccountantUserId = e.AccountantUserId,
                AccountantName = e.AccountantUser?.UserName
            });

            return Ok(result);
        }
        /// <summary>
        /// جلب مصروفات المستخدم الحالي
        /// </summary>
        /// <returns></returns>
        [HttpGet("MyExpenses")]
       
        public async Task<IActionResult> GetMyExpenses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var spec = new BaseSpecification<Expense>(e => e.AccountantUserId == userId);
            spec.Includes.Add(e => e.AccountantUser);
          //  spec.OrderByDescending(e => e.CreatedAt);

            var expenses = await _unitOfWork.Repository<Expense>().ListAsync(spec);

            var result = expenses.Select(e => new ExpenseResponseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Description = e.Description,
               // CreatedAt = e.CreatedAt,
                AccountantUserId = e.AccountantUserId,
                AccountantName = e.AccountantUser?.UserName
            });

            return Ok(result);
        }

        /// <summary>
        /// جلب مصروفات محاسب معين
        /// </summary>
        /// <param name="accountantUserId"></param>
        /// <returns></returns>
        [HttpGet("ByAccountant")]
       // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetExpensesByAccountant([FromQuery]string accountantUserId)
        {
            var spec = new BaseSpecification<Expense>(e => e.AccountantUserId == accountantUserId);
            spec.Includes.Add(e => e.AccountantUser);
      //      spec.OrderByDescending(e => e.CreatedAt);

            var expenses = await _unitOfWork.Repository<Expense>().ListAsync(spec);

            var result = expenses.Select(e => new ExpenseResponseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Description = e.Description,
                CreatedAt = e.DateOfCreation,
                AccountantUserId = e.AccountantUserId,
                AccountantName = e.AccountantUser?.UserName
            });

            return Ok(result);
        }


       
     
            




    }
}