using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [EnableCors]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private static readonly object _lock = new object();

        [HttpGet]
        [Route("airports")]
        public IActionResult SearchAirports(string search)
        {
            lock (_lock)
            {
                var airports = FlightStorage.SearchAiport(search);
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

                return Ok(FlightStorage.SearchFlightReq(request));
            }
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult SearchFlightById(int id)
        {
            lock (_lock)
            {
                var flight = FlightStorage.GetFlight(id);
                if (flight == null)
                    return NotFound();
                return Ok(flight);
            }
        }
    }
}