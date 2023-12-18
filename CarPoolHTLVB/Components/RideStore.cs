using static CarPoolHTLVB.Components.Rides.OfferRide;
using MySqlConnector;
using Dapper;
using System.Configuration;
using System.Security.Cryptography.Xml;

namespace CarPoolHTLVB.Components
{
    
    public class RideStore
    { 
        private string ConnString { get; set; }
        public RideStore(string connStrg)
        {
            ConnString = connStrg.Trim();
        }
        public bool SaveRide(RideModel model)
        {
            string newRideId = "newid";
            string sql = $"INSERT INTO rides VALUES('{newRideId}','{model.OffererID}', '{model.LocationFrom}', '{model.LocationTo}','{model.DepartureTime}','{model.ArrivalTime}','{model.VillagesPassed}', '{model.ClassmatesCanJoin}','{model.TeachersCanJoin}', {model.Seats}, '{model.Smoker}','{model.IsFree}','{model.Frequency}')";
         
            Console.WriteLine($"tried this cmd:{sql}");
            MySqlConnection connection = new MySqlConnection(ConnString);
            var entry = new
            {
                TimeStamp = DateTime.UtcNow,
                model.OffererID,
                model.DepartureTime,
                model.ArrivalTime,
                model.VillagesPassed,
                model.ClassmatesCanJoin,
                model.TeachersCanJoin,
                model.Seats,
                model.Smoker,
                model.IsFree,
                model.Frequency
            };

            try
            {
                connection.Open();
                connection.ExecuteAsync(sql, entry);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally { connection.Close(); } 
        }
    }
}
