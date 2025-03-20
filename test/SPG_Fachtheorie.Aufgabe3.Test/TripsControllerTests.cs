using SPG_Fachtheorie.Aufgabe2.Infrastructure;
using SPG_Fachtheorie.Aufgabe2.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Spg.Fachtheorie.Aufgabe3.API.Test
{
    [Collection("Sequential")]
    public class TripsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private static readonly DateTime _testDateTime = new DateTime(2025, 3, 19, 12, 0, 0);

        public TripsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private void GenerateFixtures(ScooterContext db)
        {
            var user = new User("test@email.com", 2);
            var scooter = new Scooter("RHM", "Jaguar", 3.5M);
            var trip1 = new Trip("TR1", user, scooter, _testDateTime, null, new Location(49.1M, 49.2M));
            var trip2 = new Trip("TR2", user, scooter, _testDateTime, _testDateTime.AddMinutes(20), new Location(49.1M, 49.2M));
            var triplog1 = new TripLog(trip1, _testDateTime.AddMinutes(30), new Location(49.12M, 49.12M), 5);
            var triplog2 = new TripLog(trip2, _testDateTime.AddMinutes(10), new Location(49.12M, 49.12M), 5);
            
            db.TripLogs.Add(triplog1);
            db.TripLogs.Add(triplog2);

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        [Theory]
        [InlineData("TR1", HttpStatusCode.OK, false)]
        [InlineData("TR1", HttpStatusCode.OK, true)]
        [InlineData("111", HttpStatusCode.BadRequest, false)]
        [InlineData("TR5", HttpStatusCode.NotFound, false)]
        public async Task GetTripByKey(string key, HttpStatusCode expectedCode, bool includeLog)
        {
            _factory.InitializeDatabase(db =>
            {
                GenerateFixtures(db);
            });
            var (statusCode, tripDto) = 
                await _factory.GetHttpContent<TripDto>($"/trips/{key}?includeLog={includeLog}");
            Assert.Equal(expectedCode, statusCode);
            if(statusCode == HttpStatusCode.OK)
            {
                Assert.NotNull(tripDto);
                if(includeLog)
                {
                    Assert.NotEmpty(tripDto.Logs);
                }
            }
        }

        [Theory]
        [InlineData("TR1", HttpStatusCode.OK, "2025-03-19", "49.2", "49.2")]
        [InlineData("111", HttpStatusCode.BadRequest, "2025-03-19", "49.2", "49.2")]
        [InlineData("TR2", HttpStatusCode.BadRequest, "2025-03-19", "49.2", "49.2")]
        [InlineData("TR5", HttpStatusCode.NotFound, "2025-03-19", "49.2", "49.2")]
        public async Task PutTripByKey(string key, HttpStatusCode expectedCode, string newTime, string newLong, string newLat)
        {
            _factory.InitializeDatabase(db =>
            {
                GenerateFixtures(db);
            });
            var (statusCode, tripDto) =
                await _factory.PatchHttpContent($"/trips/{key}", new UpdateTripCommand(
                    newTime,
                    decimal.Parse(newLong),
                    decimal.Parse(newLat)));
            Assert.Equal(expectedCode, statusCode);
            if (statusCode == HttpStatusCode.OK)
            {
                var tripFromDb = _factory.QueryDatabase(db => db.Trips.FirstOrDefault(t => t.Key == "TR1"));
                Assert.NotNull(tripFromDb);
                Assert.Equal(tripFromDb.End, DateTime.Parse("2025-03-19"));
            }
        }
    }
}
