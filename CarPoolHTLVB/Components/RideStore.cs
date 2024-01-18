﻿using static CarPoolHTLVB.Components.Rides.OfferRide;
using static CarPoolHTLVB.Components.Rides.SearchForRide;
using MySqlConnector;
using Dapper;
using System.Configuration;
using System.Security.Cryptography.Xml;

namespace CarPoolHTLVB.Components
{
    
    public class RideStore
    {
        public List<OfferRideModel> Rides = new();
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
                string sql = "SELECT Min(rideid) AS 'rideid' FROM rides";
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
            if(newRideId == 9) { newRideId = 10; }
            string sql = $"INSERT INTO rides VALUES('{newRideId}','{model.OffererID}', '{model.LocationFrom}', '{model.LocationTo}','{model.DepartureTime}','{model.ArrivalTime}','{model.VillagesPassed}', '{model.ClassmatesCanJoin}','{model.TeachersCanJoin}', {model.Seats}, '{model.Smoker}','{model.IsFree}','{model.Frequency}')";
         
            Console.WriteLine($"tried this cmd:{sql}");
            MySqlConnection connection = new MySqlConnection(ConnString);
            var entry = new
            {
                TimeStamp = DateTime.UtcNow,
                model.OffererID,
                model.LocationFrom,
                model.LocationTo,
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
        public bool GetRides(SearchForRideModel model)
        {
            using MySqlConnection connection = new(ConnString);
            string sql = "SELECT id, arrivaltime, ... FROM rides WHERE ... ORDER BY...";
            try
            {
                Rides= connection.Query<OfferRideModel>(sql).ToList();
                return true;
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); return false; }
        }
    }
}
