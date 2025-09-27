using iTeamAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Courses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Resources) 
            .ToListAsync();

        return Ok(courses);
    }

    // GET: api/Courses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Resources) 
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        return course;
    }


    // GET: api/Courses/Category/{category}
    [HttpGet("Category/{dept}")]
    public async Task<ActionResult<IEnumerable<Course>>> GetCoursesByCategory(string dept)
    {

        if (string.IsNullOrWhiteSpace(dept))
        {
            return BadRequest("Category cannot be empty.");
        }

        dept = dept.ToUpper();
        var courses = await _context.Courses
                                    .Where(c => c.Department == dept)
                                    .ToListAsync();

        if (!courses.Any())
        {
            return NotFound("No courses found for this category.");
        }

        return Ok(courses);
    }

    // POST: api/Courses
    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse(Course course)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        course.Department = course.Department.ToUpper();
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, course);
    }


    // PUT: api/Courses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, Course course)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != course.CourseId)
        {
            return BadRequest();
        }

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Courses.Any(e => e.CourseId == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Courses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/Courses/count
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCourseCount()
    {
        var count = await _context.Courses.CountAsync();
        return Ok(count);
    }

}
