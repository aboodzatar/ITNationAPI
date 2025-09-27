using iTeamAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudyPlansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET ALL STUDY PLANS ----------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyPlan>>> GetStudyPlans()
        {
            return await _context.StudyPlans.ToListAsync();
        }

        // ---------------------- GET STUDY PLAN BY ID ----------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyPlan>> GetStudyPlanById(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);

            if (studyPlan == null)
            {
                return NotFound();
            }

            return studyPlan;
        }

        // ---------------------- CREATE STUDY PLAN ----------------------
        [HttpPost]
        public async Task<ActionResult<StudyPlan>> CreateStudyPlan([FromBody] StudyPlan studyPlan)
        {
            if (studyPlan == null)
            {
                return BadRequest("Study plan cannot be null.");
            }

            _context.StudyPlans.Add(studyPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudyPlanById), new { id = studyPlan.Id }, studyPlan);
        }

        // ---------------------- UPDATE STUDY PLAN ----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyPlan(int id, [FromBody] StudyPlan studyPlan)
        {
            if (id != studyPlan.Id)
            {
                return BadRequest("Study plan ID mismatch.");
            }

            _context.Entry(studyPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyPlanExists(id))
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

        // ---------------------- DELETE STUDY PLAN ----------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyPlan(int id)
        { 
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null)
            {
                return NotFound();
            }

            _context.StudyPlans.Remove(studyPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------- HELPER METHOD TO CHECK IF STUDY PLAN EXISTS ----------------------
        private bool StudyPlanExists(int id)
        {
            return _context.StudyPlans.Any(e => e.Id == id);
        }
    }
}
