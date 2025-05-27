using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SmartRide.Data;
using SmartRide.Models;
using System.Net.Http;
using System.Security.Policy;
using System.Text;

namespace SmartRide.Hubs
{
    public class TrackHub : Hub
    {
        private readonly ApplicationDbContext context;
        private IHttpClientFactory clientFactory;

        public TrackHub(ApplicationDbContext _context, IHttpClientFactory clientFactory)
        {
            context = _context;
            this.clientFactory = clientFactory;
        }

        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }


        // send route method
        // this will call update map on both customer and driver
        // this method will be called by the driver
        public async Task SendRoute(string message)
        {

            // deserialize the message to DriverLocation object
            DriverMsg msg = JsonConvert.DeserializeObject<DriverMsg>(message);
            var location = context.Locations.FirstOrDefault(l => l.RideId == msg.rideId);

            // make request to graphhopper api to get route
            var client = clientFactory.CreateClient();
            var url = "https://graphhopper.com/api/1/route?key=" + Environment.GetEnvironmentVariable("SmartRideApiKey");
            List<List<decimal>> Points = new List<List<decimal>>();
            List<decimal> start = new List<decimal> { msg.longitude, msg.latitude };
            Points.Add(start);
            List<decimal> end = new List<decimal>();
            if (msg.finishedPickup == 0)
            {
                end.Add(location.PickupLong);
                end.Add(location.PickupLat);
            }
            else
            {
                end.Add(location.DropoffLong);
                end.Add(location.DropoffLat);
            }
            Points.Add(end);


            using StringContent jsonContent = new(
                JsonConvert.SerializeObject(new
                {
                    profile = msg.vehicleType,
                    points = Points,
                    points_encoded = false
                }),
                Encoding.UTF8,
                "application/json");

            using HttpResponseMessage response = await client.PostAsync(
                url,
                jsonContent);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            // deserialize the response to GHResponse object
            GHResponse ghResponse = JsonConvert.DeserializeObject<GHResponse>(responseBody);

            // payment calculation
            if(msg.finishedPickup == 0)
            {
                var distance = Convert.ToInt32(Math.Ceiling(ghResponse.paths[0].distance / 1000));
                var payment = new Payment
                {
                    RideId = msg.rideId,
                    Total = distance * 5000, // assuming 5000 is the rate per km
                };
                context.Payments.Add(payment);
                context.SaveChanges();
            }else if(msg.finishedPickup == 1) // pickup done
            {
                var distance = Convert.ToInt32(Math.Ceiling(ghResponse.paths[0].distance/1000));
                var payment = context.Payments.FirstOrDefault(p => p.RideId == msg.rideId);
                payment.Total += distance * 5000; // assuming 5000 is the rate per km for the dropoff
                context.Payments.Update(payment);
                context.SaveChanges();

                // update ride status to instransit
                var temp = context.Rides.Find(msg.rideId);
                temp.status = "InTransit";
                context.Rides.Update(temp);
                context.SaveChanges();

                await Clients.Group("RideGroup" + msg.rideId.ToString()).SendAsync("pickupComplete");
            }

            
            var ride = context.Rides.First(r => r.RideId == msg.rideId);
            if (ride.status == "Cancelled")
            {
                ghResponse.cancelled = true;
                responseBody = JsonConvert.SerializeObject(ghResponse);
            }
            responseBody = JsonConvert.SerializeObject(ghResponse);

            // invoke update map on client, both customer and driver
            await Clients.Group("RideGroup" + msg.rideId.ToString()).SendAsync("UpdateMap", responseBody);
        }

    }

    public class DriverMsg
    {
        public int finishedPickup { get; set; }
        public string vehicleType { get; set; }
        public int rideId { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
    }


    public class GHResponse // graph hopper response
    {
        public bool cancelled { get; set; } = false;
        public Hints hints { get; set; }
        public Info info { get; set; }
        public Path[] paths { get; set; }
    }

    public class Hints
    {
        public int visited_nodessum { get; set; }
        public int visited_nodesaverage { get; set; }
    }

    public class Info
    {
        public string[] copyrights { get; set; }
        public int took { get; set; }
    }

    public class Path
    {
        public float distance { get; set; }
        public float weight { get; set; }
        public int time { get; set; }
        public int transfers { get; set; }
        public bool points_encoded { get; set; }
        public float[] bbox { get; set; }
        public Points points { get; set; }
        public Instruction[] instructions { get; set; }
        public object[] legs { get; set; }
        public Details details { get; set; }
        public float ascend { get; set; }
        public float descend { get; set; }
        public Snapped_Waypoints snapped_waypoints { get; set; }
    }

    public class Points
    {
        public string type { get; set; }
        public float[][] coordinates { get; set; }
    }

    public class Details
    {
    }

    public class Snapped_Waypoints
    {
        public string type { get; set; }
        public float[][] coordinates { get; set; }
    }

    public class Instruction
    {
        public float distance { get; set; }
        public float heading { get; set; }
        public int sign { get; set; }
        public int[] interval { get; set; }
        public string text { get; set; }
        public int time { get; set; }
        public string street_name { get; set; }
        public float last_heading { get; set; }
    }


}
