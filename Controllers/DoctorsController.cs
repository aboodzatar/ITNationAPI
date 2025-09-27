using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTeamAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET ALL DOCTORS ----------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return Ok(doctors);
        }

        // ---------------------- GET DOCTOR BY ID ----------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }


        // ---------------------- GET DOCTORS BY DEPARTMENT ----------------------
        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorsByDepartment(string department)
        {
            var doctors = await _context.Doctors
                                        .Where(d => d.Department.ToLower() == department.ToLower())
                                        .ToListAsync();

            if (doctors == null || !doctors.Any())
            {
                return NotFound("No doctors found in this department.");
            }

            return Ok(doctors);
        }


        // ---------------------- CREATE A NEW DOCTOR ----------------------
        [HttpPost]
        public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
        }

        // ---------------------- UPDATE A DOCTOR ----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctor(int id, Doctor doctor)
        {
            if (id != doctor.Id)
                return BadRequest("Doctor ID mismatch.");

            _context.Entry(doctor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------- DELETE A DOCTOR ----------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------- GET DOCTORS COUNT ----------------------
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetDoctorCount()
        {
            var count = await _context.Doctors.CountAsync();
            return Ok(count);
        }

    }
}
