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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace TinderCloneV1 {
    class CoachService : ICoachService {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachService(ILogger log) {
            this.log = log;
        }

        /*Returns the profile of all coaches (from the student table)
          and the workload of all coaches (from the coach table) */
        public async Task<HttpResponseMessage> GetAllCoachProfiles() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            List<CoachProfile> listOfCoachProfiles = new List<CoachProfile>();

            string queryString = $@"SELECT Student.*, Coach.workload
                                    FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Coach]
                                    ON Student.studentID = Coach.studentID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /*The connection is automatically closed when going out of scope of the using block.
                    The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                    connection.Open();
                    try {
                        /* Get all profiles from the Student and Coach tables */
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");

                            //The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    /*Query was succesfully executed, but returned no data.
                                    Return response code [404 Not Found] */
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                } 
                                while (reader.Read()) {
                                    listOfCoachProfiles.Add(new CoachProfile(
                                        new Coach {
                                            studentID = GeneralFunctions.SafeGetInt(reader, 0),
                                            workload = GeneralFunctions.SafeGetInt(reader, 10)
                                        },
                                        new Student {
                                            studentID = GeneralFunctions.SafeGetInt(reader, 0),
                                            firstName = GeneralFunctions.SafeGetString(reader, 1),
                                            surName = GeneralFunctions.SafeGetString(reader, 2),
                                            phoneNumber = GeneralFunctions.SafeGetString(reader, 3),
                                            photo = GeneralFunctions.SafeGetString(reader, 4),
                                            description = GeneralFunctions.SafeGetString(reader, 5),
                                            degree = GeneralFunctions.SafeGetString(reader, 6),
                                            study = GeneralFunctions.SafeGetString(reader, 7),
                                            studyYear = GeneralFunctions.SafeGetInt(reader, 8),
                                            interests = GeneralFunctions.SafeGetString(reader, 9)
                                        }
                                    ));
                                }
                            }
                        }
                    } catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned. */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            string jsonToReturn = JsonConvert.SerializeObject(listOfCoachProfiles);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            /* Return response code [200 OK] and the requested data. */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /* Creates a new profile based on the data in the requestbody */
        public async Task<HttpResponseMessage> CreateCoachProfile(JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            PropertyInfo[] properties = typeof(Student).GetProperties();

            /* Check if the required data is present in the requestBody 
               before making the CoachProfile object */
            if(requestBodyData["coach"]["studentID"] == null 
             || requestBodyData["coach"]["workload"] == null
             || requestBodyData["student"]["studentID"] == null){
                log.LogError("Requestbody is missing the required data");
                return exceptionHandler.BadRequest(log);
            }

            /* The requestBody contains at least the required data so
                Save the request Body in a coachProfile object */
            CoachProfile coachProfile = requestBodyData.ToObject<CoachProfile>();

            /* Verify if the studentID of the "student" and the "coach" objects match.
               A [400 Bad Request] is returned if these are mismatching. */
            if (coachProfile.student.studentID != coachProfile.coach.studentID) {
                log.LogError("RequestBody has mismatching studentID for student and coach objects!");
                return exceptionHandler.BadRequest(log);
            }

            /* All fields for the Coach table are required */
            string queryString_Coach = $@"INSERT INTO [dbo].[Coach] (studentID, workload)
                                            VALUES (@studentID, @workload);";
            
            /* The SQL query for the Students table has to be dynamically generated, as it contains many optional fields.
               By manually adding the columns to the query string (if they're present in the request body) we prevent
               SQL injection and ensure no illegitimate columnnames are entered into the SQL query. */

            /* Dynamically create the INSERT INTO line of the SQL statement: */
            string queryString_Student = $@"INSERT INTO [dbo].[Student] (studentID";
            if (coachProfile.student.firstName != null)       queryString_Student += ", firstName";
            if (requestBodyData["student"]["surName"] != null)         queryString_Student += ", surName";
            if (requestBodyData["student"]["phoneNumber"] != null)     queryString_Student += ", phoneNumber";
            if (requestBodyData["student"]["photo"] != null)           queryString_Student += ", photo";
            if (requestBodyData["student"]["description"] != null)     queryString_Student += ", description";
            if (requestBodyData["student"]["degree"] != null)          queryString_Student += ", degree";
            if (requestBodyData["student"]["study"] != null)           queryString_Student += ", study";
            if (requestBodyData["student"]["studyYear"] != null)       queryString_Student += ", studyYear";
            if (requestBodyData["student"]["interests"] != null)       queryString_Student += ", interests";
            queryString_Student += ") ";

            /* Dynamically create the VALUES line of the SQL statement: */
            queryString_Student += "VALUES (@studentID";
            if (requestBodyData["student"]["firstName"] != null)       queryString_Student += ", @firstName";
            if (requestBodyData["student"]["surName"] != null)         queryString_Student += ", @surName";
            if (requestBodyData["student"]["phoneNumber"] != null)     queryString_Student += ", @phoneNumber";
            if (requestBodyData["student"]["photo"] != null)           queryString_Student += ", @photo";
            if (requestBodyData["student"]["description"] != null)     queryString_Student += ", @description";
            if (requestBodyData["student"]["degree"] != null)          queryString_Student += ", @degree";
            if (requestBodyData["student"]["study"] != null)           queryString_Student += ", @study";
            if (requestBodyData["student"]["studyYear"] != null)       queryString_Student += ", @studyYear";
            if (requestBodyData["student"]["interests"] != null)       queryString_Student += ", @interests";
            queryString_Student += ");";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /*The connection is automatically closed when going out of scope of the using block.
                      The connection may fail to open, in which case return a [503 Service Unavailable]. */
                    connection.Open();

                    try {
                        /*Insert profile into the Student table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place */
                            command.Parameters.Add("studentID", System.Data.SqlDbType.Int).Value = coachProfile.student.studentID;
                            if (requestBodyData["student"]["firstName"] != null)       command.Parameters.Add("@firstName",    System.Data.SqlDbType.VarChar).Value =      coachProfile.student.firstName;
                            if (requestBodyData["student"]["surName"] != null)         command.Parameters.Add("@surName",      System.Data.SqlDbType.VarChar).Value =      coachProfile.student.surName;
                            if (requestBodyData["student"]["phoneNumber"] != null)     command.Parameters.Add("@phoneNumber",  System.Data.SqlDbType.VarChar).Value =      coachProfile.student.phoneNumber;
                            if (requestBodyData["student"]["photo"] != null)           command.Parameters.Add("@photo",        System.Data.SqlDbType.VarChar).Value =      coachProfile.student.photo;
                            if (requestBodyData["student"]["description"] != null)     command.Parameters.Add("@description",  System.Data.SqlDbType.VarChar).Value =      coachProfile.student.description;
                            if (requestBodyData["student"]["degree"] != null)          command.Parameters.Add("@degree",       System.Data.SqlDbType.VarChar).Value =      coachProfile.student.degree;
                            if (requestBodyData["student"]["study"] != null)           command.Parameters.Add("@study",        System.Data.SqlDbType.VarChar).Value =      coachProfile.student.study;
                            if (requestBodyData["student"]["studyYear"] != null)       command.Parameters.Add("@studyYear",    System.Data.SqlDbType.Int).Value =          coachProfile.student.studyYear;
                            if (requestBodyData["student"]["interests"] != null)       command.Parameters.Add("@interests",    System.Data.SqlDbType.VarChar).Value =      coachProfile.student.interests;
                            
                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            // command.ExecuteReader();
                        }

                        /*Insert profile into the Coach table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Coach, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place */
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = coachProfile.coach.studentID;
                            command.Parameters.Add("@workload", System.Data.SqlDbType.Int).Value = coachProfile.coach.workload;

                            log.LogInformation($"Executing the following query: {queryString_Coach}");

                            // command.ExecuteReader();
                        }
                    } catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned.
                           Reasons for this failure may include a PK violation (entering an already existing studentID). */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            log.LogInformation($"{HttpStatusCode.Created} | Profile created succesfully.");

            /* Return response code [201 Created]. */
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        /* Returns the profile of the coach (from the student table) 
          and the workload of the coach (from the coach table) */
        public async Task<HttpResponseMessage> GetCoachProfileByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            CoachProfile newCoachProfile = new CoachProfile();

            string queryString = $@"SELECT Student.*, Coach.workload
                                    FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Coach]
                                    ON Student.studentID = Coach.studentID
                                    WHERE Student.studentID = @coachID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /* The connection is automatically closed when going out of scope of the using block.
                       The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                    connection.Open();

                    try {
                        /* Get profile from the Student and Coach tables */
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place */
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            /* The Query may fail, in which case a [400 Bad Request] is returned. */
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    /* Query was succesfully executed, but returned no data.
                                       Return response code [404 Not Found] */
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                } 
                                while (reader.Read()) {
                                    newCoachProfile = new CoachProfile(
                                        new Coach {
                                            studentID = GeneralFunctions.SafeGetInt(reader, 0),
                                            workload = GeneralFunctions.SafeGetInt(reader, 10)
                                        },
                                        new Student {
                                            studentID = GeneralFunctions.SafeGetInt(reader, 0),
                                            firstName = GeneralFunctions.SafeGetString(reader, 1),
                                            surName = GeneralFunctions.SafeGetString(reader, 2),
                                            phoneNumber = GeneralFunctions.SafeGetString(reader, 3),
                                            photo = GeneralFunctions.SafeGetString(reader, 4),
                                            description = GeneralFunctions.SafeGetString(reader, 5),
                                            degree = GeneralFunctions.SafeGetString(reader, 6),
                                            study = GeneralFunctions.SafeGetString(reader, 7),
                                            studyYear = GeneralFunctions.SafeGetInt(reader, 8),
                                            interests = GeneralFunctions.SafeGetString(reader, 9)
                                        }
                                    );
                                }
                            }
                        }
                    } catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned. */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log); 
                    }
                }
            } catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log); 
            }

            var jsonToReturn = JsonConvert.SerializeObject(newCoachProfile);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            /* Return response code [200 OK] and the requested data. */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /* Deletes the Coach from the Coach table
           then deletes the Coach from Studen table */
        public async Task<HttpResponseMessage> DeleteCoachProfileByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //Query string used to delete the coach from the coach table
            string queryString_Coach = $@"DELETE
                                            FROM [dbo].[Coach]
                                            WHERE studentID = @coachID";

            //Query string used to delete the coach from the Students table
            string queryString_Student = $@"DELETE
                                            FROM [dbo].[Student]
                                            WHERE studentID = @coachID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        //Delete the coach from the Coach table
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString_Coach, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString_Coach}");

                            int affectedRows = command.ExecuteNonQuery();

                            //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Coach table.");
                                return exceptionHandler.NotFound();
                            }
                        }

                        //Delete the profile from the Students table
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            int affectedRows = command.ExecuteNonQuery();

                            //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Student table.");
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

        //Returns the workload of the coach (from the coach table)
        public async Task<HttpResponseMessage> GetCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            Coach newCoach = new Coach();

            string queryString = $@"SELECT *
                                    FROM [dbo].[Coach]
                                    WHERE studentID = @coachID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        //Get data from the Coach table by studentID
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
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
                                    newCoach = new Coach {
                                        studentID = GeneralFunctions.SafeGetInt(reader, 1),
                                        workload = GeneralFunctions.SafeGetInt(reader, 2)
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

            var jsonToReturn = JsonConvert.SerializeObject(newCoach);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        //Updates the workload of the coach (in the coach table)
        public async Task<HttpResponseMessage> UpdateCoachByID(int coachID, JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            Coach newCoach = requestBodyData.ToObject<Coach>();

            //newCoach.workload will be 0 if the requestbody contains no "workload" parameter,
            //in which case [400 Bad Request] is returned.
            if(requestBodyData["workload"] == null) {
                log.LogError("Requestbody contains no workload.");
                return exceptionHandler.BadRequest(log);
            }

            string queryString = $@"UPDATE [dbo].[Coach]
                                    SET workload = @workload
                                    WHERE studentID = @coachID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        //Update the workload
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@workload", System.Data.SqlDbType.Int).Value = newCoach.workload;
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = command.ExecuteNonQuery();

                            //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data updated succesfully.");

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
