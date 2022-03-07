using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [EnableCors]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private static readonly object _lock = new object();
        private readonly FlightPlannerDbContext _context;

        public CustomerApiController(FlightPlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("airports")]
        public IActionResult SearchAirports(string search)
        {
            lock (_lock)
            {
                var airports = SearchAirport(search);

                return Ok(airports);
            }
        }

        [HttpPost]
        [Route("flights/search")]
        public IActionResult SearchFlights(SearchFlightRequest request)
        {
            lock (_lock)
            {
                if (!FlightStorage.IsValidFlight(request))
                {
                    return BadRequest();
                }

                return Ok(FlightStorage.SearchFlightReq(request, _context));
            }
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult SearchFlightById(int id)
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

        private List<Airport> SearchAirport(string search)
        {
            lock (_lock)
            {
                search = search.ToLower().Trim();
                var fromAirports = _context.Flights.Where(x =>
                        x.From.AirportName.ToLower().Trim().Contains(search) ||
                        x.From.City.ToLower().Trim().Contains(search) ||
                        x.From.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.From)
                    .ToList();

                var toAirports = _context.Flights.Where(x =>
                        x.To.AirportName.ToLower().Trim().Contains(search) ||
                        x.To.City.ToLower().Trim().Contains(search) ||
                        x.To.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.To)
                    .ToList();

                return fromAirports.Concat(toAirports).ToList();
            }
        }
    }
}