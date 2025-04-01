using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;
        public CategoryService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<CategoryModel>> GetAllCategory()
        {
            var categories = await _context.Categories.ToListAsync();
            return _mapper.Map<List<CategoryModel>>(categories);
        }
    }
}
