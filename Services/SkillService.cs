using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.EntityFrameworkCore;

namespace MyCourse.Services
{
    public class SkillService : ISkillService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        public SkillService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SkillModel>> GetSkillsByCourseIdAsync(int courseId)
        {
            
            var courseSkills = await _context.CourseSkills
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Skill) 
                .ToListAsync();
            var skillModels = _mapper.Map<List<SkillModel>>(courseSkills.Select(cs => cs.Skill).ToList());

            return skillModels;
        }
    }
}
