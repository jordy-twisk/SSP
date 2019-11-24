using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    class CoachTutorantService : ICoachTutorantService {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachTutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        //Changes the status of a CoachTutorantConnection.
        public async Task<HttpResponseMessage> UpdateConnection(JObject ctObject) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //Verify if all parameters for the CoachTutorantConnection exist.
            //One or more parameters may be missing, in which case a [400 Bad Request] is returned.
            if (ctObject["studentIDTutorant"] == null 
                || ctObject["studentIDCoach"] == null 
                || ctObject["status"] == null) {
                log.LogError("Requestbody is missing data for the CoachTutorantConnection table!");
                return exceptionHandler.BadRequest(log);
            }
            
            /* Make a Connection entity from the requestBody after checking the required fields */
            CoachTutorantConnection coachTutorantConnection = ctObject.ToObject<CoachTutorantConnection>();

            string queryString = $@"UPDATE [dbo].[CoachTutorantConnection]
                                    SET status = @status
                                    WHERE studentIDTutorant = @studentIDTutorant AND studentIDCoach = @studentIDCoach;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentIDTutorant", SqlDbType.Int).Value = coachTutorantConnection.studentIDTutorant;
                            command.Parameters.Add("@studentIDCoach", SqlDbType.Int).Value = coachTutorantConnection.studentIDCoach;
                            command.Parameters.Add("@status", SqlDbType.VarChar).Value = coachTutorantConnection.status;

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            //The studentIDs must be incorrect if no rows were affected, return a [404 Not Found].
                            if(affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data updated succesfully.");

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        //Returns all connections of a specific coach
        public async Task<HttpResponseMessage> GetAllConnectionsByCoachID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            List<CoachTutorantConnection> listOfCoachTutorantConnections = new List<CoachTutorantConnection>();

            string queryString = $@"SELECT *
                                    FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDCoach = @coachID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        //Get all connections from the CoachTutorantConnections table for a specific coach
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            //The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    //Query was succesfully executed, but returned no data.
                                    //Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                } 
                                while (reader.Read()) {
                                    listOfCoachTutorantConnections.Add(new CoachTutorantConnection {
                                        //Reader 0 contains coachTutorantConnectionID key (of the database),
                                        //this data is irrelevant for the user.
                                        studentIDTutorant = GeneralFunctions.SafeGetInt(reader, 1),
                                        studentIDCoach = GeneralFunctions.SafeGetInt(reader, 2),
                                        status = GeneralFunctions.SafeGetString(reader, 3)
                                    });
                                }
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfCoachTutorantConnections);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        //Delete all connections of a specific coach
        public async Task<HttpResponseMessage> DeleteConnectionByCoachID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //Query string used to delete the coach from the coach table
            string queryString = $@"DELETE FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDcoach = @coachID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        //Delete all connections from a specific coach in the CoachTutorantConnection table
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            //The studentIDs must be incorrect if no rows were affected, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully.");

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        //Returns the connection of a specific tutorant
        public async Task<HttpResponseMessage> GetConnectionByTutorantID(int tutorantID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            CoachTutorantConnection coachTutorantConnection = new CoachTutorantConnection();

            string queryString = $@"SELECT * FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDTutorant = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        //Get connection from the CoachTutorantConnections table for a specific tutorant
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            //The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    //Query was succesfully executed, but returned no data.
                                    //Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                }
                                while (reader.Read()) {
                                    coachTutorantConnection = new CoachTutorantConnection {
                                        //Reader 0 contains coachTutorantConnectionID key (of the database),
                                        //this data is irrelevant for the user.
                                        studentIDTutorant = GeneralFunctions.SafeGetInt(reader, 1),
                                        studentIDCoach = GeneralFunctions.SafeGetInt(reader, 2),
                                        status = GeneralFunctions.SafeGetString(reader, 3)
                                    };
                                }
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log); 
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(coachTutorantConnection);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        //Create a new connection between a tutorant and coach
        public async Task<HttpResponseMessage> CreateConnectionByTutorantID(int tutorantID, JObject tTocConnection) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //Verify if all parameters for the CoachTutorantConnection exist.
            //One or more parameters may be missing, in which case a [400 Bad Request] is returned.
            if (tTocConnection["studentIDTutorant"] == null 
                || tTocConnection["studentIDCoach"] == null 
                || tTocConnection["status"] == null) {
                log.LogError("Requestbody is missing data for the CoachTutorantConnection table!");
                return exceptionHandler.BadRequest(log);
            }

            /* Make a Connection entity from the requestBody after checking the required fields */
            CoachTutorantConnection coachTutorantConnection = tTocConnection.ToObject<CoachTutorantConnection>();

            string queryString = $@"INSERT INTO [dbo].[CoachTutorantConnection] (studentIDTutorant, studentIDCoach, status)
                                    VALUES (@studentIDTutorant, @studentIDCoach, @status);";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@status", SqlDbType.VarChar).Value = coachTutorantConnection.status;
                            command.Parameters.Add("@studentIDTutorant", SqlDbType.Int).Value = coachTutorantConnection.studentIDTutorant;
                            command.Parameters.Add("@studentIDCoach", SqlDbType.Int).Value = coachTutorantConnection.studentIDCoach;

                            log.LogInformation($"Executing the following query: {queryString}");

                            await command.ExecuteNonQueryAsync();
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Connection created succesfully.");

            //Return response code [201 Created].
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        //Delete the connections of a specific tutorant
        public async Task<HttpResponseMessage> DeleteConnectionByTutorantID(int tutorantID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //Query string used to delete the coach from the coach table
            string queryString = $@"DELETE FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDtutorant = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        //Delete the connection from a specific tutorant in the CoachTutorantConnection table
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            //The studentIDs must be incorrect if no rows were affected, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
                            }
                        }

                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully.");

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}