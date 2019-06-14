using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(UserManager<IdentityUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<JsonResult> UserAlreadyExistsAsync([Bind(Prefix = "Input.Username")]string Username)
        {          
            var result =
                await _userManager.FindByNameAsync(Username);
            return Json(result==null);
        }

        [HttpGet("/Account/all")]
        public async Task<List<IdentityUser>> GetAll()
        {
            var result =
                await _userManager.Users.ToListAsync();
            return result;
        }

        [HttpGet("/Account/{id}")]
        public async Task<JsonResult> GetUserName(string id)
        {
            var result =
                await _userManager.FindByIdAsync(id);
            return Json(new {result.UserName, result.Email, result.PhoneNumber});
        }

        [HttpGet("/Account/current")]
        public async Task<JsonResult> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return Json(user?.Id);
        }

        //Account/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("/Account/{id}")]
        public async Task<IActionResult> DeleteUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }
            
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{id}'.");
            }
            

            _logger.LogInformation("User with ID '{UserId}' has been deleted by Admin.", id);

            return Ok();
        }
    }
}