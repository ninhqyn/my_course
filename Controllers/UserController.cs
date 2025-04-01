using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/<UserController>/5
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserModel>> Get()
        {
            var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));

            if (userId == 0)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userService.GetUser(userId);
            if(user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        
        
        
    }
}
