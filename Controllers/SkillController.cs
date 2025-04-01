using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;
        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }
        [HttpGet("{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SkillModel>>> GetAllSkillByCategoryid(int courseId)
        {
            try
            {
                var skills = await _skillService.GetSkillsByCourseIdAsync(courseId);
                if (skills == null || skills.Count == 0)
                {
                    return NotFound();
                }

                return Ok(skills);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }

    }
}
