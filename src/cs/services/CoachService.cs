﻿using System;
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

        //Returns the profile of all coaches (from the student table)
        //and the workload of all coaches (from the coach table)
        public async Task<HttpResponseMessage> GetAllCoachProfiles() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            List<CoachProfile> listOfCoachProfiles = new List<CoachProfile>();

            string queryString = $@"SELECT Student.*, Coach.workload
                                    FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Coach]
                                    ON Student.studentID = Coach.studentID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        //The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        listOfCoachProfiles.Add(new CoachProfile(
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
                                        ));
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

            var jsonToReturn = JsonConvert.SerializeObject(listOfCoachProfiles);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> CreateCoachProfile() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            Coach newCoach = new Coach();
            User newUser = new User();
            JObject jObject = new JObject();

            string body = await req.Content.ReadAsStringAsync();

            throw new NotImplementedException();

        }

        //Returns the profile of the coach (from the student table) 
        //and the workload of the coach (from the coach table)
        public async Task<HttpResponseMessage> GetCoachProfileByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            CoachProfile newCoachProfile = new CoachProfile();

            string queryString = $@"SELECT Student.*, Coach.workload
                                    FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Coach]
                                    ON Student.studentID = Coach.studentID
                                    WHERE Student.studentID = @coachID;";

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

        //Deletes the Coach from the Coach table
        //then deletes the Coach from Studen table
        public async Task<HttpResponseMessage> DeleteCoachProfileByID(int coachID) {
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

        //Returns the workload of the coach (from the coach table)
        public async Task<HttpResponseMessage> GetCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            Coach newCoach = new Coach();

            string queryString = $@"SELECT *
                                    FROM [dbo].[Coach]
                                    WHERE studentID = @coachID;";

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
                                        newCoach = new Coach {
                                            studentID = reader.GetInt32(0),
                                            workload = reader.GetInt32(1)
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

            var jsonToReturn = JsonConvert.SerializeObject(newCoach);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code 200 and the requested data
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        //Updates the workload of the coach (in the coach table)
        /*
         * Example requestbody for use in Postman (testing):
            {
                "studentID": 500000, 
                "workload": 5
            } 
        */
        public async Task<HttpResponseMessage> UpdateCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            Coach newCoach;
            JObject jObject;

            //Read from the requestBody
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                newCoach = jObject.ToObject<Coach>();
            }

            //newCoach.workload will be 0 if the requestbody contains no "workload" parameter,
            //return response code 400 if this error occurs
            if(jObject["workload"] == null) {
                log.LogError("Requestbody contains no workload!");
                return exceptionHandler.BadRequest(log);
            }

            string queryString = $@"UPDATE [dbo].[Coach]
                                    SET workload = @workload
                                    WHERE studentID = @coachID;";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        //The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@workload", System.Data.SqlDbType.Int).Value = newCoach.workload;
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString}");

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

            log.LogInformation($"{HttpStatusCode.NoContent} | Data updated succesfully");

            //Return response code 204
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
