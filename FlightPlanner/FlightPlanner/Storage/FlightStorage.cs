using FlightPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static List<Flight> _flights = new List<Flight>();
        private static int _id;
        private static readonly object _lock = new object();
        private static readonly FlightPlannerDbContext _context;

        public static Flight AddFlight(AddFlightRequest request)
        {
            lock (_lock)
            {
                var flight = new Flight
                {
                    From = request.From,
                    To = request.To,
                    ArrivalTime = request.ArrivalTime,
                    DepartureTime = request.DepartureTime,
                    Carrier = request.Carrier,
                    Id = ++_id
                };
                _flights.Add(flight);

                return flight;
            }
        }

        public static Flight ConvertToFlight(AddFlightRequest request)
        {
            lock (_lock)
            {
                var flight = new Flight
                {
                    From = request.From,
                    To = request.To,
                    ArrivalTime = request.ArrivalTime,
                    DepartureTime = request.DepartureTime,
                    Carrier = request.Carrier,
                };

                return flight;
            }
        }

        public static PageResult SearchFlightReq(SearchFlightRequest request, FlightPlannerDbContext context)
        {
            lock (_lock)
            {
                var flights = context.Flights.Where(x =>
                    x.From.AirportName == request.From &&
                    x.To.AirportName == request.To &&
                    x.DepartureTime.Substring(0, 10) == request.DepartureDate).ToList();

                return new PageResult(flights);
            }
        }

        public static List<Airport> SearchAirport(string search)
        {
            lock (_lock)
            {
                search = search.ToLower().Trim();
                var fromAirports = _flights.Where(x =>
                    x.From.AirportName.ToLower().Trim().Contains(search) ||
                    x.From.City.ToLower().Trim().Contains(search) ||
                    x.From.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.From)
                    .ToList();

                var toAirports = _flights.Where(x =>
                    x.To.AirportName.ToLower().Trim().Contains(search) ||
                    x.To.City.ToLower().Trim().Contains(search) ||
                    x.To.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.To)
                    .ToList();

                return fromAirports.Concat(toAirports).ToList();
            }
        }

        public static Flight GetFlight(int id)
        {
            lock (_lock)
            {
                return _flights.SingleOrDefault(x => x.Id == id);
            }
        }

        public static void DeleteFlight(int id)
        {
            lock (_lock)
            {
                var flight = GetFlight(id);
                if (flight != null)
                    _flights.Remove(flight);
            }
        }

        public static void ClearFlights()
        {
            lock (_lock)
            {
                _flights.Clear();
                _id = 0;
            }
        }

        public static bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _flights.Any(x =>
                     x.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                     x.DepartureTime == request.DepartureTime &&
                     x.ArrivalTime == request.ArrivalTime &&
                     x.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim() &&
                     x.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim());
            }
        }

        public static bool IsValid(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (request == null)
                    return false;

                if (string.IsNullOrEmpty(request.ArrivalTime) || string.IsNullOrEmpty(request.DepartureTime) || string.IsNullOrEmpty(request.Carrier))
                    return false;

                if (request.From == null || request.To == null)
                    return false;

                if (string.IsNullOrEmpty(request.From.AirportName) || string.IsNullOrEmpty(request.From.City) ||
                    string.IsNullOrEmpty(request.From.Country) || string.IsNullOrEmpty(request.To.AirportName) ||
                    string.IsNullOrEmpty(request.To.City) || string.IsNullOrEmpty(request.To.Country))
                    return false;

                if (request.From.Country.ToLower().Trim() == request.To.Country.ToLower().Trim() &&
                    request.From.City.ToLower().Trim() == request.To.City.ToLower().Trim() &&
                    request.From.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim())
                    return false;

                var arrivalTime = DateTime.Parse(request.ArrivalTime);
                var departureTime = DateTime.Parse(request.DepartureTime);

                if (arrivalTime <= departureTime)
                    return false;

                return true;
            }
        }

        public static bool IsValidFlight(SearchFlightRequest request)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.DepartureDate))
                    return false;

                if (request.From == request.To)
                    return false;

                return true;
            }
        }


    }
}