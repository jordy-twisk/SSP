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
    class CoachService : ICoachService {
        private readonly string environmentString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public async Task<HttpResponseMessage> CreateCoach() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            Coach newCoach = new Coach();
            User newUser = new User();
            JObject jObject = new JObject();

            string body = await req.Content.ReadAsStringAsync();

            throw new NotImplementedException();

        }

        public async Task<HttpResponseMessage> DeleteCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);

            //Query string used to delete the coach from the coach table
            string queryString_Coach = $@"DELETE
                                            FROM [dbo].[Coach]
                                            WHERE studentID = @coachID";

            //Query string used to delete the coach from the Students table
            string queryString_Student = $@"DELETE
                                            FROM [dbo].[Student]
                                            WHERE studentID = @coachID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        //The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        //Delete the coach from the Coach table
                        using (SqlCommand command = new SqlCommand(queryString_Coach, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString_Coach}");

                            command.ExecuteNonQuery();
                        }

                        //Delete the profile from the Students table
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            command.ExecuteNonQuery();
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

            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");

            //Return response code 204
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        public Task<HttpResponseMessage> GetAllCoaches() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetCoachAndWorkloadByID(int coachID) {
            throw new NotImplementedException();
        }

        //Returns the profile of the coach (from the student table) 
        //and the workload of the coach (from the coach table)
        public async Task<HttpResponseMessage> GetCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            CoachProfile newCoachProfile = new CoachProfile();

            string queryString = $@"SELECT Student.*, Coach.workload
                                    FROM [dbo].[Coach], [dbo].[Student]
                                    WHERE Coach.studentID = @coachID AND Student.studentID = @coachID;";

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
                                        newCoachProfile = new CoachProfile(
                                            new Coach {
                                                studentID = reader.GetInt32(0),
                                                workload = reader.GetInt32(10)
                                            },
                                            new User {
                                                studentID = reader.GetInt32(0),
                                                firstName = reader.GetString(1),
                                                surName = reader.GetString(2),
                                                phoneNumber = reader.GetString(3),
                                                photo = reader.GetString(4),
                                                description = reader.GetString(5),
                                                degree = reader.GetString(6),
                                                study = reader.GetString(7),
                                                studyYear = reader.GetInt32(8),
                                                interests = reader.GetString(9)
                                            }
                                        );
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

            var jsonToReturn = JsonConvert.SerializeObject(newCoachProfile);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code 200 and the requested data
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public Task<HttpResponseMessage> UpdateCoachAndWorkloadByID(int coachID) {
            throw new NotImplementedException();
        }
    }
}
