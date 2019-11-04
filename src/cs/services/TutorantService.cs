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

        public TutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }
        // Create a new profile based on the data in the request body.
        public async Task<HttpResponseMessage> CreateTutorantProfile() {
            exceptionHandler = new ExceptionHandler(0);
            TutorantProfile tutorantProfile;
            JObject jObject;

            // Read from the request body.
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                tutorantProfile = jObject.ToObject<TutorantProfile>();
            }

            // Verify if all parameters for the Tutorant table exist.
            // One or more parameters may be missing, in which case a [400 Bad Request] is returned.
            if (jObject["tutorant"]["studentID"] == null) {
                log.LogError("Requestbody is missing data for the tutorant table!");
                return exceptionHandler.BadRequest(log);
            }

            // Verify if all required parameters for the Student table exist,
            // return response code 400 if one or more is missing.
            if (jObject["user"]["studentID"] == null) {
                log.LogError("Requestbody is missing data for the student table!");
                return exceptionHandler.BadRequest(log);
            }

            //Verify if the studentID of the "user" and the "tutorant" objects match.
            //A [400 Bad Request] is returned if these are mismatching.
            if (tutorantProfile.tutorant.studentID != tutorantProfile.user.studentID){
                log.LogError("RequestBody has mismatching studentID for user and tutorant objects!");
                return exceptionHandler.BadRequest(log);
            }
            
            // All fields for the Tutorant table are required.
            string queryStringTutorant = $@"INSERT INTO [dbo].[Tutorant] (studentID) VALUES (@studentID);";

            // The SQL query for the Students table has to be dynamically generated, as it contains many optional fields.
            // By manually adding the columns to the query string (if they're present in the request body) we prevent
            // SQL injection and ensure no illegitimate columnnames are entered into the SQL query.

            // Dynamically create the INSERT INTO line of the SQL statement:
            string queryString_Student = $@"INSERT INTO [dbo].[Student] (studentID";
            if (jObject["user"]["firstName"] != null) queryString_Student += ", firstName";
            if (jObject["user"]["surName"] != null) queryString_Student += ", surName";
            if (jObject["user"]["phoneNumber"] != null) queryString_Student += ", phoneNumber";
            if (jObject["user"]["photo"] != null) queryString_Student += ", photo";
            if (jObject["user"]["description"] != null) queryString_Student += ", description";
            if (jObject["user"]["degree"] != null) queryString_Student += ", degree";
            if (jObject["user"]["study"] != null) queryString_Student += ", study";
            if (jObject["user"]["studyYear"] != null) queryString_Student += ", studyYear";
            if (jObject["user"]["interests"] != null) queryString_Student += ", interests";
            queryString_Student += ") ";

            // Dynamically create the VALUES line of the SQL statement:
            queryString_Student += "VALUES (@studentID";
            if (jObject["user"]["firstName"] != null) queryString_Student += ", @firstName";
            if (jObject["user"]["surName"] != null) queryString_Student += ", @surName";
            if (jObject["user"]["phoneNumber"] != null) queryString_Student += ", @phoneNumber";
            if (jObject["user"]["photo"] != null) queryString_Student += ", @photo";
            if (jObject["user"]["description"] != null) queryString_Student += ", @description";
            if (jObject["user"]["degree"] != null) queryString_Student += ", @degree";
            if (jObject["user"]["study"] != null) queryString_Student += ", @study";
            if (jObject["user"]["studyYear"] != null) queryString_Student += ", @studyYear";
            if (jObject["user"]["interests"] != null) queryString_Student += ", @interests";
            queryString_Student += ");";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case return a [503 Service Unavailable].
                    connection.Open();

                    try {
                        // Insert profile into the Student table
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("studentID", System.Data.SqlDbType.Int).Value = tutorantProfile.user.studentID;
                            if (jObject["user"]["firstName"] != null)   command.Parameters.Add("@firstName", System.Data.SqlDbType.NVarChar).Value =     tutorantProfile.user.firstName;
                            if (jObject["user"]["surName"] != null)     command.Parameters.Add("@surName", System.Data.SqlDbType.NVarChar).Value =       tutorantProfile.user.surName;
                            if (jObject["user"]["phoneNumber"] != null) command.Parameters.Add("@phoneNumber", System.Data.SqlDbType.NVarChar).Value =   tutorantProfile.user.phoneNumber;
                            if (jObject["user"]["photo"] != null)       command.Parameters.Add("@photo", System.Data.SqlDbType.VarChar).Value =          tutorantProfile.user.photo;
                            if (jObject["user"]["description"] != null) command.Parameters.Add("@description", System.Data.SqlDbType.VarChar).Value =    tutorantProfile.user.description;
                            if (jObject["user"]["degree"] != null)      command.Parameters.Add("@degree", System.Data.SqlDbType.NVarChar).Value =        tutorantProfile.user.degree;
                            if (jObject["user"]["study"] != null)       command.Parameters.Add("@study", System.Data.SqlDbType.NVarChar).Value =         tutorantProfile.user.study;
                            if (jObject["user"]["studyYear"] != null)   command.Parameters.Add("@studyYear", System.Data.SqlDbType.Int).Value =          tutorantProfile.user.studyYear;
                            if (jObject["user"]["interests"] != null)   command.Parameters.Add("@interests", System.Data.SqlDbType.VarChar).Value =      tutorantProfile.user.interests;
                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            command.ExecuteNonQuery();
                        }

                        // Insert profile into the Tutorant table.
                        using (SqlCommand command = new SqlCommand(queryStringTutorant, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = tutorantProfile.tutorant.studentID;
                            log.LogInformation($"Executing the following query: {queryStringTutorant}");

                            command.ExecuteNonQuery();
                        }
                    } catch (SqlException e) {
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        // Reasons for this failure may include a PK violation (entering an already existing studentID).
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Profile created succesfully.");

            // Return response code [201 Created].
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        // Deletes the Tutorant from the Tutorant table.
        // then deletes the Tutorant from Student table.
        public async Task<HttpResponseMessage> DeleteTutorantProfileByID(int tutorantID) {
            exceptionHandler = new ExceptionHandler(tutorantID);

            // Query string used to delete the tutorant from the Tutorant table.
            string queryStringTutorant = $@"DELETE FROM [dbo].[Tutorant] WHERE studentID = @tutorantID";

            // Query string used to delete the tutorant from the Students table.
            string queryStringStudent = $@"DELETE FROM [dbo].[Student] WHERE studentID = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        // Delete the tutorant from the tutorant table.
                        using (SqlCommand command = new SqlCommand(queryStringTutorant, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryStringTutorant}");

                            int affectedRows = command.ExecuteNonQuery();

                            // The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Tutorant table.");
                                return exceptionHandler.NotFoundException(log);
                            }
                        }

                        // Delete the profile from the Students table.
                        using (SqlCommand command = new SqlCommand(queryStringStudent, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryStringStudent}");

                            int affectedRows = command.ExecuteNonQuery();

                            //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Student table.");
                                return exceptionHandler.NotFoundException(log);
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

            // Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Returns the profile of all tutorants (from the student table).
        public async Task<HttpResponseMessage> GetAllTutorantProfiles() {
            exceptionHandler = new ExceptionHandler(0);
            List<TutorantProfile> listOfTutorantProfiles = new List<TutorantProfile>();

            string queryString = $@"SELECT Student.* FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Tutorant] ON Student.studentID = Tutorant.studentID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");

                            // The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    // Query was succesfully executed, but returned no data.
                                    // Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                } else {
                                    while (reader.Read()) {
                                        listOfTutorantProfiles.Add(new TutorantProfile(
                                            new Tutorant {
                                                studentID = GeneralFunctions.SafeGetInt(reader, 0),
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
                        }
                    } catch (SqlException e) {
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfTutorantProfiles);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        // Returns the profile of the tutorant (from the student table).
        public async Task<HttpResponseMessage> GetTutorantProfileByID(int tutorantID) {
            exceptionHandler = new ExceptionHandler(tutorantID);
            TutorantProfile newTutorantProfile = new TutorantProfile();

            string queryString = $@"SELECT Student.* FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Tutorant] 
                                    ON Student.studentID = Tutorant.studentID
                                    WHERE Student.studentID = @tutorantID;";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            //The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Query was succesfully executed, but returned no data.
                                    //Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        newTutorantProfile = new TutorantProfile(
                                            new Tutorant {
                                                studentID = GeneralFunctions.SafeGetInt(reader, 0)
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

            var jsonToReturn = JsonConvert.SerializeObject(newTutorantProfile);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            //Return response code [200 OK] and the requested data.
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }
    }
}
