using System.Security.Claims;
using iTeamAPI.DTOs;
using iTeamAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iTeamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ITFactoryRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ITFactoryRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET BY REQUEST TYPE ----------------------
        [HttpGet("requesttype/{requestType}")]
        public async Task<ActionResult<IEnumerable<ITFactoryRequest>>> GetByRequestType(string requestType)
        {
            var requests = await _context.ITFactoryRequests
                                        .Where(r => r.RequestType.ToLower() == requestType.ToLower() && r.Status == "Approved")
                                        .ToListAsync();

            if (requests == null || !requests.Any())
                return NotFound("No requests found with the specified RequestType.");

            return Ok(requests);
        }

        // ---------------------- DELETE ALL REJECTED REQUESTS ----------------------
        [HttpDelete("delete-rejected")]
        public async Task<IActionResult> DeleteAllRejectedRequests()
        {
            var rejectedRequests = await _context.ITFactoryRequests
                                                 .Where(r => r.Status.ToLower() == "rejected")
                                                 .ToListAsync();

            if (rejectedRequests.Count == 0)
                return NotFound("No rejected requests found to delete.");

            _context.ITFactoryRequests.RemoveRange(rejectedRequests);
            await _context.SaveChangesAsync();

            return Ok("All rejected requests have been deleted.");
        }

        // ---------------------- GET BY STATUS ----------------------
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<ITFactoryRequest>>> GetByStatus(string status)
        {
            var requests = await _context.ITFactoryRequests
                                        .Where(r => r.Status.ToLower() == status.ToLower())
                                        .ToListAsync();

            if (requests == null || !requests.Any())
                return NotFound($"No requests found with the status '{status}'.");

            return Ok(requests);
        }

        // ---------------------- CREATE A NEW REQUEST ----------------------
        [HttpPost]
        public async Task<ActionResult<ITFactoryRequest>> CreateRequest([FromBody] ITFactoryRequest request)
        {
            if (request == null)
                return BadRequest("Request data cannot be null.");

            // Get current user's ID from token/claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            // Set the user ID from the server
            request.UserId = userId;

            // Validate type-specific fields here if needed

            _context.ITFactoryRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByRequestType), new { requestType = request.RequestType }, request);
        }
        // ---------------------- UPDATE AN EXISTING REQUEST ----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] ITFactoryRequest request)
        {
            if (id != request.Id)
                return BadRequest("Request ID mismatch.");

            var existingRequest = await _context.ITFactoryRequests.FindAsync(id);
            if (existingRequest == null)
                return NotFound("Request not found.");

            // Update request properties
            existingRequest.RequestType = request.RequestType;
            existingRequest.Details = request.Details;
            existingRequest.WhatsAppLink = request.WhatsAppLink;
            existingRequest.Status = request.Status;
            existingRequest.AdminNotes = request.AdminNotes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------- DELETE A REQUEST BY ID ----------------------
        [HttpDelete("{id}")]
        [Authorize] // Make sure the user is logged in
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.ITFactoryRequests.FindAsync(id);
            if (request == null)
                return NotFound("Request not found.");

            // Get current user ID from token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Only allow the user who created it or an Admin
            var isAdmin = User.IsInRole("Admin");
            if (request.UserId != currentUserId && !isAdmin)
                return Forbid("You can only delete your own requests.");

            _context.ITFactoryRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        // ---------------------- DELETE ALL IT FACTORY REQUESTS ----------------------
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllRequests()
        {
            var allRequests = await _context.ITFactoryRequests.ToListAsync();

            if (allRequests.Count == 0)
                return NotFound("No requests found to delete.");

            _context.ITFactoryRequests.RemoveRange(allRequests);
            await _context.SaveChangesAsync();

            return Ok("All IT Factory requests have been deleted.");
        }


        // ---------------------- GET ALL REQUESTS BY CURRENT USER ----------------------
        [HttpGet("my-requests")]
        public async Task<ActionResult<IEnumerable<ITFactoryRequest>>> GetMyRequests()
        {
            // Get the user ID from the token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            // Query the database for this user's requests
            var userRequests = await _context.ITFactoryRequests
                                             .Where(r => r.UserId == userId)
                                             .ToListAsync();

            if (userRequests == null || !userRequests.Any())
                return NotFound("No requests found for the current user.");

            return Ok(userRequests);
        }

        // ---------------------- GET TOTAL IT REQUESTS COUNT ----------------------
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetTotalRequestsCount()
        {
            var count = await _context.ITFactoryRequests.CountAsync();
            return Ok(count);
        }

        // ---------------------- GET ALL IT FACTORY REQUESTS ----------------------
        [HttpGet]
        [Authorize] // Optional: only admins can access
        public async Task<ActionResult<IEnumerable<ITFactoryRequest>>> GetAllRequests()
        {
            var allRequests = await _context.ITFactoryRequests.ToListAsync();

            if (!allRequests.Any())
                return NotFound("No IT Factory requests found.");

            return Ok(allRequests);
        }

        // ---------------------- UPDATE REQUEST STATUS ----------------------
        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var request = await _context.ITFactoryRequests.FindAsync(id);
            if (request == null)
                return NotFound("Request not found.");

            // Validate status value
            var validStatuses = new[] { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid status. Use 'Pending', 'Approved', or 'Rejected'.");
            }

            // Update status and admin notes
            request.Status = dto.Status;
            request.AdminNotes = dto.AdminNotes; 

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the request.");
            }

            return NoContent();
        }


    }
}
