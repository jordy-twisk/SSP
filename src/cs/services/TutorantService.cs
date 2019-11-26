using System;
using System.Data;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    class TutorantService : ITutorantService {

        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly ILogger log;

        public TutorantService(ILogger log) {
            this.log = log;
        }
        // Create a new profile based on the data in the request body.
        public async Task<HttpResponseMessage> CreateTutorantProfile(JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            JObject tutorantProfile = requestBodyData.SelectToken("tutorant").ToObject<JObject>();
            JObject studentProfile = requestBodyData.SelectToken("student").ToObject<JObject>();

            // Verify if all parameters for the tables exist.
            // One or more parameters may be missing, in which case a [400 Bad Request] is returned.
            if (tutorantProfile["studentID"] == null ||
                studentProfile["studentID"] == null) {
                log.LogError("Requestbody is missing the required data");
                return exceptionHandler.BadRequest(log);
            }

            Tutorant newTutorant = tutorantProfile.ToObject<Tutorant>();
            Student newStudent = studentProfile.ToObject<Student>();

            // Verify if the studentID of the "user" and the "tutorant" objects match.
            // A [400 Bad Request] is returned if these are mismatching.
            if (newTutorant.studentID != newStudent.studentID){
                log.LogError("RequestBody has mismatching studentID for student and tutorant objects!");
                return exceptionHandler.BadRequest(log);
            }
            
            // All fields for the Tutorant table are required.
            string queryStringTutorant = $@"INSERT INTO [dbo].[Tutorant] (studentID) VALUES (@studentID);";

            // The SQL query for the Students table has to be dynamically generated, as it contains many optional fields.
            // By manually adding the columns to the query string (if they're present in the request body) we prevent
            // SQL injection and ensure no illegitimate columnnames are entered into the SQL query.

            // Dynamically create the INSERT INTO line of the SQL statement:
            string queryString_Student = $@"INSERT INTO [dbo].[Student] (";
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"{property.Name}, ";
                    }
                }
            }
            queryString_Student = RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ") ";

            // Dynamically create the VALUES line of the SQL statement:
            queryString_Student += "VALUES (";
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"@{property.Name}, ";
                    }
                }
            }

            queryString_Student = RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ");";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case return a [503 Service Unavailable].
                    int studentCreated = 0;

                    connection.Open();

                    try {
                        // Insert profile into the Student table
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {

                            // Parameters are used to ensure no SQL injection can take place.
                            dynamic dObject = newStudent;
                            AddSqlInjection(studentProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            studentCreated = command.ExecuteNonQuery();
                        }

                        // Insert profile into the Tutorant table.
                        using (SqlCommand command = new SqlCommand(queryStringTutorant, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            dynamic dObject = newTutorant;
                            AddSqlInjection(tutorantProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryStringTutorant}");

                            if (studentCreated == 1) {
                                command.ExecuteNonQuery();
                            }
                            else {
                                log.LogError($"Cannot create tutorant profile, student does not exists");
                                return exceptionHandler.BadRequest(log);
                            }
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
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            // Query string used to delete the tutorant from the Tutorant table.
            string queryStringTutorant = $@"DELETE FROM [dbo].[Tutorant] WHERE studentID = @tutorantID";

            // Query string used to delete the tutorant from the Students table.
            string queryStringStudent = $@"DELETE FROM [dbo].[Student] WHERE studentID = @tutorantID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        // Delete the tutorant from the tutorant table.
                        using (SqlCommand command = new SqlCommand(queryStringTutorant, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", SqlDbType.Int).Value = tutorantID;

                            log.LogInformation($"Executing the following query: {queryStringTutorant}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            // The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Tutorant table.");
                                return exceptionHandler.NotFound();
                            }
                        }

                        // Delete the profile from the Students table.
                        using (SqlCommand command = new SqlCommand(queryStringStudent, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryStringStudent}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

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

            // Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Returns the profile of all tutorants (from the student table).
        public async Task<HttpResponseMessage> GetAllTutorantProfiles() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            List<TutorantProfile> listOfTutorantProfiles = new List<TutorantProfile>();

            string queryString = $@"SELECT Student.* FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Tutorant] 
                                    ON Student.studentID = Tutorant.studentID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");

                            // The Query may fail, in which case a [400 Bad Request] is returned.
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    // Query was succesfully executed, but returned no data.
                                    // Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                }
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
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            TutorantProfile newTutorantProfile = new TutorantProfile();

            string queryString = $@"SELECT Student.* FROM [dbo].[Student]
                                    INNER JOIN [dbo].[Tutorant] 
                                    ON Student.studentID = Tutorant.studentID
                                    WHERE Student.studentID = @tutorantID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            command.Parameters.Add("@tutorantID", SqlDbType.Int).Value = tutorantID;
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

        public string RemoveLastCharacters(string queryString, int NumberOfCharacters) {
            queryString = queryString.Remove(queryString.Length - NumberOfCharacters);
            return queryString;
        }
        public void AddSqlInjection(JObject rboy, dynamic dynaObject, SqlCommand cmd) {
            foreach (JProperty property in rboy.Properties()) {
                foreach (PropertyInfo props in dynaObject.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        var type = Nullable.GetUnderlyingType(props.PropertyType) ?? props.PropertyType;

                        if (type == typeof(string)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.VarChar).Value = props.GetValue(dynaObject, null);
                        }
                        if (type == typeof(int)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.Int).Value = props.GetValue(dynaObject, null);
                        }
                        if (type == typeof(DateTime)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.DateTime).Value = props.GetValue(dynaObject, null);
                        }
                    }
                }
            }
        }
    }
}
