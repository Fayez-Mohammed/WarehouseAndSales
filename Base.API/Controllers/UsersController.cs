using Base.Services.Interfaces;
using Base.Shared.DTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
   // [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "StoreManager,SystemAdmin,Accountant")]
    [Authorize(Policy = "ActiveUserOnly")]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UsersController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }
        /// <summary>
        /// جلب جميع المستخدمين مع خيارات التصفية والصفحة
        /// </summary>
        /// <param name="search"></param>
        /// <param name="userType"></param>
        /// <param name="isActive"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET: api/users
        [HttpGet("list")]
        public async Task<ActionResult<UserListDto>> GetAll(
            [FromQuery] string? search,
            [FromQuery] UserTypes? userType,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _userProfileService.GetAllAsync(search, userType, isActive, page, pageSize);
            return Ok(result);
        }
        /// <summary>
        /// جلب مستخدم بواسطة المعرف
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // GET: api/users/{id}
        [HttpGet("get-user")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var user = await _userProfileService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
        /// <summary>
        /// انشذاء مستخدم جديد
        /// </summary>
        /// <param name="userType"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // POST: api/users
        [HttpPost("create")]
        public async Task<ActionResult<UserDto>> Create([FromQuery] UserTypes? userType,[FromBody] CreateUserRequest request)
        {
            
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userProfileService.CreateAsync(request);
            return Ok(user);
        }
        /// <summary>
        ///  تحديث بيانات المستخدم
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // PUT: api/users/{id}
        [HttpPut("update")]
        public async Task<ActionResult<UserDto>> Update(string id, UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userProfileService.UpdateAsync(id, request);
            if (user == null) return NotFound();
            return Ok(user);
        }
        /// <summary>
        ///  عكس حالة التفعيل للمستخدم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // PATCH: api/users/{id}/toggle-active
        [HttpPatch("toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var success = await _userProfileService.ToggleActiveAsync(id);
            if (!success) return Forbid();
            return Ok();
        }
        /// <summary>
        ///  حذف مستخدم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // DELETE: api/users/{id}
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            var success = await _userProfileService.DeleteAsync(id);
            if (!success) return Forbid();
            return Ok();
        }
        /// <summary>
        /// تغيير كلمة مرور المستخدم
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // PATCH: api/users/{id}/change-password
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(newPassword)) throw new ArgumentNullException(nameof(newPassword));
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return BadRequest("Password must be at least 6 characters.");

            var success = await _userProfileService.ChangePasswordAsync(id, newPassword);
            if (!success) return Forbid();
            return Ok();
        }
    }
}

