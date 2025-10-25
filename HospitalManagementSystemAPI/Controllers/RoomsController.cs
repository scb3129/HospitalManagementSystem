using HospitalManagementSystemAPI.Data;
using HospitalManagementSystemAPI.DTOs;
using HospitalManagementSystemAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Patient)
                .Select(r => new RoomDTO
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    BedNumber = r.BedNumber,
                    IsOccupied = r.IsOccupied,
                    PatientId = r.PatientId,
                    PatientName = r.Patient != null ? r.Patient.Name : null
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // POST: api/rooms
        [HttpPost]
        public async Task<ActionResult<RoomDTO>> PostRoom(Room room)
        {
            room.IsOccupied = room.PatientId != null;
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRooms), new { id = room.RoomId }, room);
        }

        // DELETE: api/rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/rooms/5/assign/2
        [HttpPost("{roomId}/assign/{patientId}")]
        public async Task<IActionResult> AssignPatient(int roomId, int patientId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound("Room not found");

            room.PatientId = patientId;
            room.IsOccupied = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/rooms/5/unassign
        [HttpPost("{roomId}/unassign")]
        public async Task<IActionResult> UnassignPatient(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound("Room not found");

            room.PatientId = null;
            room.IsOccupied = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
