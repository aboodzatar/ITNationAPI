// Controllers/MyCoursesController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using iTeamAPI.DTOs;
using iTeamAPI.Models;
using System.Security.Claims;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MyCoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MyCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET: api/mycourses ----------------------
        [HttpGet]
        public async Task<IActionResult> GetAllForCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var myCourses = await _context.MyCourses
                .Where(mc => mc.UserId == userId)
                .Include(mc => mc.Course)
                    .ThenInclude(c => c.Resources) // <-- This loads the resources
                .Select(mc => new MyCourseDto
                {
                    UserId = userId,
                    CourseId = mc.CourseId,
                    IsFavorite = mc.IsFavorite,
                    NotifyMe = mc.NotifyMe,
                    Course = mc.Course // Now includes Resources
                })
                .ToListAsync();

            return Ok(myCourses);
        }

        // ---------------------- POST: api/mycourses ----------------------
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(CreateMyCourseDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _context.MyCourses
                .FirstOrDefaultAsync(mc => mc.UserId == userId && mc.CourseId == dto.CourseId);

            if (existing != null)
            {
                // Remove from favorites
                _context.MyCourses.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Removed from favorites" });
            }

            // Add to favorites
            var newEntry = new MyCourse
            {
                UserId = userId,
                CourseId = dto.CourseId,
                IsFavorite = true,
                NotifyMe = false
            };

            _context.MyCourses.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites" });
        }

        // ---------------------- DELETE: api/mycourses/all ----------------------
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var myCourses = await _context.MyCourses
                .Where(mc => mc.UserId == userId)
                .ToListAsync();

            if (!myCourses.Any())
                return NotFound("No courses found.");

            _context.MyCourses.RemoveRange(myCourses);
            await _context.SaveChangesAsync();

            return Ok("All courses deleted.");
        }

        // ---------------------- GET: api/mycourses/isfavorite/{courseId} ----------------------
        [HttpGet("isfavorite/{courseId}")]
        public async Task<ActionResult<bool>> IsFavorite(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();


            var isFavorite = await _context.MyCourses
                .AnyAsync(mc => mc.UserId == userId && mc.CourseId == courseId && mc.IsFavorite);

            return Ok(isFavorite);
        }

    }
}