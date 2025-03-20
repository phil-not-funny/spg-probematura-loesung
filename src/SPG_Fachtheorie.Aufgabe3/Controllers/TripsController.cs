using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe2.Infrastructure;
using SPG_Fachtheorie.Aufgabe2.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System.Text.RegularExpressions;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ScooterContext _db;
        private readonly Regex _tripKeyRegex = new Regex(@"^TR[0-9]+$", RegexOptions.Compiled);

        public TripsController(ScooterContext db)
        {
            _db = db;
        }

        //     GET /trips/{key}?includeLog=true und
        //     PATCH /trips/{key}
        // ATHOR: PHILIP SCHRENK

        private async Task<Trip?> GetTrip(string key) => 
            await _db.Trips.Include(t => t.User).Include(t => t.Scooter).Include(t => t.TripLogs).FirstOrDefaultAsync(t => t.Key == key);

        private TripDto GetDtoFromDbEntry(Trip trip, bool includeLog) => 
            new(trip.Key,
                  trip.User.Email,
                  trip.Scooter.ManufacturerId,
                  trip.Begin.ToString(),
                  trip.End != null ? trip.End.ToString() : "",
                  includeLog ? trip.TripLogs.Select(l => new TripLogDto(
                      l.Timestamp.ToString(),
                      l.Location.Longitude,
                      l.Location.Latitude,
                      l.MileageInMeters)).ToList() : new());

        [HttpGet("{key}")]
        public async Task<IActionResult> GetTripByKey(string key, [FromQuery] bool includeLog = false)
        {
            if (!_tripKeyRegex.IsMatch(key))
                return BadRequest("Der Key ist ungültig!");

            var trip = await GetTrip(key);

            if (trip == null)
                return NotFound("Der Key ist keinem Trip zugewiesen.");

            return Ok(GetDtoFromDbEntry(trip, includeLog));
        }

        [HttpPatch("{key}")]
        public async Task<IActionResult> PatchTripsByKey(string key, [FromBody] UpdateTripCommand updateTripCommand)
        {
            if (!_tripKeyRegex.IsMatch(key))
                return BadRequest("Der Key ist ungültig!");

            var trip = await GetTrip(key);

            if (trip == null)
                return NotFound("Der Key ist keinem Trip zugewiesen.");

            if (trip.End.HasValue)
                return BadRequest("Der Trip wurde bereits beednet!");

            trip.End = DateTime.Parse(updateTripCommand.End);
            trip.ParkingLocation = new Location(updateTripCommand.Logitude, updateTripCommand.Latitude);
            await _db.SaveChangesAsync();

            return Ok(GetDtoFromDbEntry(trip, false));
        }

    }
}
