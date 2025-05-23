﻿using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IInstructorService
    {
        Task<List<InstructorModel>> GetAllInstructorsByCourseIdAsync(int courseId);
        Task<InstructorModel> GetInstructorByIdAsync(int instructorId);
    }
}
