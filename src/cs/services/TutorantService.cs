using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;


namespace TinderCloneV1 {
    class TutorantService : ITutorantService {

        private readonly string environmentString = Environment.GetEnvironmentVariable("sqldb_connection");
        private ExceptionHandler exceptionHandler;
        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;
        private string queryString;

        public TutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }
        public async Task<HttpResponseMessage> CreateTutorant() {
            exceptionHandler = new ExceptionHandler(0);
            Tutorant tutorant;
            JObject jObject = new JObject();

            // Read from the request body.
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                tutorant = jObject.ToObject<Tutorant>();
            }

            // Verify if all parameters for the Tutorant table exist,
            // return response code 400 if one or more is missing.
            if (jObject["tutorant"]["studentID"] == null) {
                log.LogError("Requestbody is missing data for the tutorant table!");
                return exceptionHandler.BadRequest(log);
            }

            queryString = $@"INSERT INTO [dbo].[Tutorant] (studentID) VALUES (@studentID);";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        // The connection is automatically closed when going out of scope of the using block.
                        connection.Open();

                        // Execute the sql command.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = tutorant.studentID;
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException e) {
                        // Return response code 503.
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                // Return response code 400.
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Tutorant created succesfully");

            // Return response code 201
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        // Delete tutorant from the Tutorant table.
        public async Task<HttpResponseMessage> DeleteTutorantByID(int tutorantID) {
            // Query string used to delete the tutorant from the tutorant table.
            queryString = $@"DELETE FROM [dbo].[Tutorant] WHERE studentID = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        // The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        // Delete the tutorant from the Tutorant table.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException e) {
                        //Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                //Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }
            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");

            //Return response code 204
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Return all tutorants from the Tutorant table.
        public async Task<HttpResponseMessage> GetAllTutorants() {
            List<Tutorant> listOfTutorants = new List<Tutorant>();
            queryString = $"SELECT * FROM [dbo].[Tutorant]";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        // The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                }
                                else {
                                    while (reader.Read()) {
                                        listOfTutorants.Add(new Tutorant {studentID = reader.GetInt32(0)});
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException e) {
                        // Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                // Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfTutorants);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> GetTutorantByID(int tutorantID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(tutorantID);
            Tutorant newTutorant = new Tutorant();

            queryString = $"SELECT * FROM [dbo].[Tutorant] WHERE studentID = @tutorantID;";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        // The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = @tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    // Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                }
                                else {
                                    while (reader.Read()) {
                                        newTutorant = new Tutorant { studentID = reader.GetInt32(0) };
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException e) {
                        //Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                //Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(newTutorant);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code 200 and the requested data
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }
    }
}
