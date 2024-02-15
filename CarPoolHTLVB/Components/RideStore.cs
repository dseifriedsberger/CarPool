using static CarPoolHTLVB.Components.Rides.OfferRide;
using static CarPoolHTLVB.Components.Rides.SearchForRide;
using MySqlConnector;
using Dapper;
using System.Configuration;
using System.Security.Cryptography.Xml;
using System.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using CarPoolHTLVB.Components.Rides;

namespace CarPoolHTLVB.Components
{

    public class RideStore
    {
        public List<OfferRideModel> Rides;
        private string ConnString { get; set; }
        public RideStore(string connStrg)
        {
            ConnString = connStrg.Trim();
        }

        private int GetLastRideID()
        {
            using MySqlConnection connection = new(ConnString);
            int id = -1;
            try
            {
                connection.Open();
                string sql = "SELECT Max(rideid) AS 'rideid' FROM rides";
                MySqlCommand cmd = new(sql, connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id = Convert.ToInt32(reader["rideid"]);
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally { connection.Close(); }
            return id;
        }
        public bool SaveRide(OfferRideModel model)
        {
            int newRideId = GetLastRideID() + 10;
            if (newRideId == 9) { newRideId = 10; }
            model.RideId = newRideId;
            string sql = $"INSERT INTO rides (RideId, OffererId, LocationFrom, LocationTo, DepartureTime, ArrivalTime, VillagesPassed, ClassmatesCanJoin, TeachersCanJoin, FreeSeats, Smoker,IsFree,Frequency) VALUES('{model.RideId}','{model.OffererID}', '{model.LocationFrom}', '{model.LocationTo}','{model.DepartureTime}','{model.ArrivalTime}','{model.VillagesPassed}', '{model.ClassmatesCanJoin}','{model.TeachersCanJoin}', {model.FreeSeats}, '{model.Smoker}','{model.IsFree}','{model.Frequency}')";

            Console.WriteLine($"tried this cmd:{sql}");
            MySqlConnection connection = new MySqlConnection(ConnString);
            var entry = new
            {
                //TimeStamp = DateTime.UtcNow,
                model.OffererID,
                model.LocationFrom,
                model.LocationTo,
                model.DepartureTime,
                model.ArrivalTime,
                model.VillagesPassed,
                model.ClassmatesCanJoin,
                model.TeachersCanJoin,
                model.FreeSeats,
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


        public bool GetRides(SearchForRideModel model)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                connection.Open();

                var parameters = new
                {
                    locationFrom = model.LocationFrom,
                    locationTo = model.LocationTo,
                    searchedDate = model.DepartureTime,
                    isFreeParam = model.IsFree,
                    smokerParam = model.Smoker
                };
                Rides = connection.Query<OfferRideModel>("FindRide", parameters, commandType: CommandType.StoredProcedure).ToList();
            }

            return true;
        } 

        public bool AddToRequestIDs(int rideId, string userId)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                try
                {
                    connection.Open();

                    string updateQuery = @"
                    UPDATE rides
                    SET RequestIDs = 
                        CASE
                            WHEN RequestIDs IS NULL THEN @UserId
                            WHEN RequestIDs NOT LIKE @UserIdPattern THEN CONCAT_WS(';', RequestIDs, @UserId)
                            ELSE RequestIDs
                        END
                    WHERE RideId = @RideId;
                "; 
                    var parameters = new { RideId = rideId, UserId = userId, UserIdPattern = $"%;{userId}%" };

                    int affectedRows = connection.Execute(updateQuery, parameters);

                    Console.WriteLine($"Tried this command: {updateQuery}");

                    return affectedRows == 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    // Handle the error appropriately
                    return false;
                }
            }
        }
        public bool RequestRide(int rideId, AuthenticationState authState)
        {
            string userid = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                connection.Open();
                int freeSeats = GetFreeSeats(rideId);
                string requestedIds = GetRequestIDs(rideId);
                if(AvaiableSeats(freeSeats, requestedIds))
                {
                    if( AddToRequestIDs(rideId, userid))
                    { return true; }
                } 
            }
            return false;
        }

        private bool AvaiableSeats(int freeSeats, string requestedIds)
        {
            string[] splittedReqIds = requestedIds.Split(';'); 
            return splittedReqIds.Length - 1 < freeSeats;
        } 
        public int GetFreeSeats(int rideId)
        {
            int freeSeats = 0; 
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                string query = "SELECT FreeSeats FROM rides WHERE RideId = @RideId"; 
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RideId", rideId);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        freeSeats = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    // Handle the error appropriately
                }
            }

            return freeSeats;
        }
        public string GetRequestIDs(int rideId)
        {
            string requestIDs = ""; 
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                string query = "SELECT RequestIDs FROM rides WHERE RideId = @RideId"; 
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RideId", rideId);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        requestIDs = Convert.ToString(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message); 
                }
            }

            return requestIDs;
        }
        public List<RequestedRideList.Ride> CreateList(AuthenticationState authState)
        {
            string userid = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            using (MySqlConnection connection = new MySqlConnection(ConnString))
            {
                string query = $"SELECT LocationFrom, LocationTo, DepartureTime, OffererId FROM rides WHERE FIND_IN_SET('{userid}', REPLACE(RequestIDs, ';', ',')) > 0;";
                return connection.Query<RequestedRideList.Ride>(query).ToList();
            }
        }
    }
}
