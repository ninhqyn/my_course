using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService moduleService;
        public ModuleController(IModuleService moduleService)
        {
            this.moduleService = moduleService;
        }
        [HttpGet("{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SkillModel>>> GetAllSkillByCategoryid(int courseId)
        {
            try
            {
                var modules = await moduleService.GetAllModulesByCourseIdAsync(courseId);
                if (modules == null || modules.Count == 0)
                {
                    return NotFound();
                }

                return Ok(modules);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }
        [HttpGet("user/{courseId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserModule>>> GetAllUserModules(int courseId)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }
                var userModules = await moduleService.GetAllUserModuleByCourseIdAsync(courseId, userId);
                if (userModules == null || userModules.Count == 0)
                {
                    return NotFound();
                }
                return Ok(userModules);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}
