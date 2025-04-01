using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Services
{
    public class UserService : IUserService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;
        public UserService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserModel> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) { 
            return null;
            }
            return _mapper.Map<UserModel>(user);
        }
    }
}
