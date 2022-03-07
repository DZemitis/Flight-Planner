using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FlightPlanner.Controllers
{
    [Route("admin-api")]
    [EnableCors]
    [ApiController]
    public class AdminApiController : ControllerBase
    {
        private static readonly object _lock = new object();
        private readonly FlightPlannerDbContext _context;

        public AdminApiController(FlightPlannerDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [Route("Flights/{id}")]
        public IActionResult GetFlights(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(x => x.From)
                    .Include(x => x.To)
                    .SingleOrDefault(x => x.Id == id);
                if (flight == null)
                    return NotFound();

                return Ok(flight);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("flights")]
        public IActionResult AddFlights(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (!FlightStorage.IsValid(request))
                    return BadRequest();

                if (Exists(request))
                    return Conflict();

                var flight = FlightStorage.ConvertToFlight(request);
                _context.Flights.Add(flight);
                _context.SaveChanges();

                return Created("", flight);
            }
        }

        [HttpDelete]
        [Route("Flights/{id}")]
        public IActionResult DeleteFlights(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(x => x.From)
                    .Include(x => x.To)
                    .SingleOrDefault(x => x.Id == id);

                if (flight != null)
                {
                    _context.Flights.Remove(flight);
                    _context.SaveChanges();
                }

                return Ok();
            }
        }

        private bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _context.Flights.Any(x =>
                x.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                x.DepartureTime == request.DepartureTime &&
                x.ArrivalTime == request.ArrivalTime &&
                x.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim() &&
                x.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim());
            }
        }
    }
}
