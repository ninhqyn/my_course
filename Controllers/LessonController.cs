﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        // GET: api/lesson/module/{moduleId}
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<List<LessonModel>>> GetLessonsByModuleIdAsync(int moduleId)
        {
            try
            {
                var lessons = await _lessonService.GetLessonByModuleIdAsync(moduleId);

                if (lessons == null || lessons.Count == 0)
                {
                    return NotFound("No lessons found for the given module.");
                }

                return Ok(lessons);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/lesson/{lessonId}
        [HttpGet("{lessonId}")]
        public async Task<ActionResult<LessonModel>> GetLessonById(int lessonId)
        {
            try
            {
                var lesson = await _lessonService.GetLessonModuleById(lessonId);

                if (lesson == null)
                {
                    return NotFound("Lesson not found.");
                }

                return Ok(lesson);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("progress")]
        [Authorize]
        public async Task<ActionResult<LessonProgressModel>> CreateOrUpdateProgress(CreateProgressModel model)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                if (model == null)
                {
                    return BadRequest("Invalid progress data.");
                }

                var result = await _lessonService.CreateOrUpdateLessonProgressAsync(userId, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
