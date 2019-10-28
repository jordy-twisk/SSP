using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    class CoachTutorantService : ICoachTutorantService {
        private readonly string environmentString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachTutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateConnection() {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> GetAllCoachConnections(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            List<CoachTutorantConnection> listOfCoachTutorantConnections = new List<CoachTutorantConnection>();

            string queryString = $@"SELECT *
                                    FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDcoach = @coachID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        //The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        listOfCoachTutorantConnections.Add(new CoachTutorantConnection {
                                            studentIDTutorant = reader.GetInt32(0),
                                            studentIDCoach = reader.GetInt32(1),
                                            status = reader.GetString(2)
                                        });
                                    }
                                }
                            }
                        }
                    } catch (SqlException e) {
                        //Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            } catch (SqlException e) {
                //Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfCoachTutorantConnections);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code 200 and the requested data
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public Task<HttpResponseMessage> DeleteCoachConnection(int coachID) {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> GetTutorantConnectionByID(int tutorantID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(tutorantID);
            CoachTutorantConnection coachTutorantConnection = new CoachTutorantConnection();

            string queryString = $@"SELECT *
                                    FROM [dbo].[CoachTutorantConnection]
                                    WHERE studentIDTutorant = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        //The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        coachTutorantConnection = new CoachTutorantConnection {
                                            studentIDTutorant = reader.GetInt32(0),
                                            studentIDCoach = reader.GetInt32(1),
                                            status = reader.GetString(2)
                                        };
                                    }
                                }
                            }
                        }
                    } catch (SqlException e) {
                        //Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            } catch (SqlException e) {
                //Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(coachTutorantConnection);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code 200 and the requested data
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public Task<HttpResponseMessage> UpdateConnectionByID(int tutorantID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteTutorantConnection(int tutorantID) {
            throw new NotImplementedException();
        }
    }
}
