using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SPG_Fachtheorie.Aufgabe2.Infrastructure;
using SPG_Fachtheorie.Aufgabe2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SPG_Fachtheorie.Aufgabe2.Services
{
    public class ScooterService
    {
        private readonly ScooterContext _db;
        public record TripInfo(decimal DistanceTraveled, decimal TripCost);
        public ScooterService(ScooterContext db)
        {
            _db = db;
        }
        
        // ATHOR: PHILIP SCHRENK
        public Dictionary<int, List<TripInfo>> CalculateTripInfos()
        {
            return _db.Users
                .Include(u => u.Trips)
                    .ThenInclude(t => t.TripLogs)
                .Include(u => u.Trips)
                    .ThenInclude(t => t.Scooter)
               .ToList()
               .Select(u => new
               {
                   u.Id,
                   Trips = u.Trips.Where(t => t.End != null)
                            .Select(t => new TripInfo(
                                (t.TripLogs.Max(l => l.MileageInMeters) - t.TripLogs.Min(l => l.MileageInMeters)) / 1000M,
                                Math.Max(0, t.Scooter.PricePerKilometer * ((t.TripLogs.Max(l => l.MileageInMeters) - t.TripLogs.Min(l => l.MileageInMeters)) / 1000M - t.User.FreeKilometers)))).ToList()
               })
               .ToDictionary(
                    c => c.Id, 
                    c => c.Trips);
                
            
        }
    }
}