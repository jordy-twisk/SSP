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
    class CoachService : ICoachService {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly ILogger log;

        public CoachService(ILogger log) {
            this.log = log;
        }

        /* Returns the profile of all coaches (from the student table)
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
                                            studentID = SafeReader.SafeGetInt(reader, 0),
                                            workload = SafeReader.SafeGetInt(reader, 10)
                                        },
                                        new Student {
                                            studentID = SafeReader.SafeGetInt(reader, 0),
                                            firstName = SafeReader.SafeGetString(reader, 1),
                                            surName = SafeReader.SafeGetString(reader, 2),
                                            phoneNumber = SafeReader.SafeGetString(reader, 3),
                                            photo = SafeReader.SafeGetString(reader, 4),
                                            description = SafeReader.SafeGetString(reader, 5),
                                            degree = SafeReader.SafeGetString(reader, 6),
                                            study = SafeReader.SafeGetString(reader, 7),
                                            studyYear = SafeReader.SafeGetInt(reader, 8),
                                            interests = SafeReader.SafeGetString(reader, 9)
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
            DatabaseFunctions databaseFunctions = new DatabaseFunctions();
            /* Split the requestBody in a Coach and Student entity */
            JObject coachProfile = requestBodyData.SelectToken("coach").ToObject<JObject>();
            JObject studentProfile = requestBodyData.SelectToken("student").ToObject<JObject>();

            /* Check if the required data is present in the requestBody 
               before making the CoachProfile object */
            if (coachProfile["studentID"] == null 
             || coachProfile["workload"] == null
             || studentProfile["studentID"] == null){
                log.LogError("Requestbody is missing the required data");
                return exceptionHandler.BadRequest(log);
            }

            /* Create the entities from the split-requestBody fro readability */
            Coach newCoach = coachProfile.ToObject<Coach>();
            Student newStudent = studentProfile.ToObject<Student>();

            /* Verify if the studentID of the "student" and the "coach" objects match.
               A [400 Bad Request] is returned if these are mismatching. */
            if (newStudent.studentID != newCoach.studentID) {
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
            string queryString_Student = $@"INSERT INTO [dbo].[Student] (";
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"{property.Name}, ";
                    }
                }
            }

            queryString_Student = databaseFunctions.RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ") ";

            /* Dynamically create the VALUES line of the SQL statement: */
            queryString_Student += "VALUES (";
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"@{property.Name}, ";
                    }
                }
            }

            queryString_Student = databaseFunctions.RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ");";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /*The connection is automatically closed when going out of scope of the using block.
                      The connection may fail to open, in which case return a [503 Service Unavailable]. */
                    int studentCreated = 0;

                    connection.Open();

                    try {
                        /*Insert profile into the Student table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place 
                            To ensure generic code, a dynamic object is made to make a new Entity and be passed into the injection function */
                            dynamic dObject = newStudent;
                            databaseFunctions.AddSqlInjection(studentProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            // PREVIOUSLY: await command.ExecuteReaderAsync();
                            studentCreated =  command.ExecuteNonQuery();
                        }

                        /*Insert profile into the Coach table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Coach, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place
                               To ensure generic code, a dynamic object is made to make a new Entity and be passed into the injection function*/
                            dynamic dObject = newCoach;
                            databaseFunctions.AddSqlInjection(coachProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString_Coach}");

                            /* Is the student query affected 0 rows (i.e.: Student did not create then
                               the coach cannot exists as well, so dont make the coach*/
                            if (studentCreated == 1) {
                                // PREVIOUSLY: await command.ExecuteReaderAsync();
                                command.ExecuteNonQuery();
                            }
                            else {
                                log.LogError($"Cannot create coach profile, student does not exists");
                                return exceptionHandler.BadRequest(log);
                            }
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
                            command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;

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
                                            studentID = SafeReader.SafeGetInt(reader, 0),
                                            workload = SafeReader.SafeGetInt(reader, 10)
                                        },
                                        new Student {
                                            studentID = SafeReader.SafeGetInt(reader, 0),
                                            firstName = SafeReader.SafeGetString(reader, 1),
                                            surName = SafeReader.SafeGetString(reader, 2),
                                            phoneNumber = SafeReader.SafeGetString(reader, 3),
                                            photo = SafeReader.SafeGetString(reader, 4),
                                            description = SafeReader.SafeGetString(reader, 5),
                                            degree = SafeReader.SafeGetString(reader, 6),
                                            study = SafeReader.SafeGetString(reader, 7),
                                            studyYear = SafeReader.SafeGetInt(reader, 8),
                                            interests = SafeReader.SafeGetString(reader, 9)
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
                            command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;

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
                            command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;

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

        /* Returns the workload of the coach (from the coach table) */
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
                                    newCoach = new Coach {
                                        studentID = SafeReader.SafeGetInt(reader, 1),
                                        workload = SafeReader.SafeGetInt(reader, 2)
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

        /* Updates the workload of the coach (in the coach table) */ 
        public async Task<HttpResponseMessage> UpdateCoachByID(int coachID, JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            //newCoach.workload will be 0 if the requestbody contains no "workload" parameter,
            //in which case [400 Bad Request] is returned.
            if(requestBodyData["workload"] == null) {
                log.LogError("Requestbody contains no workload.");
                return exceptionHandler.BadRequest(log);
            }

            Coach newCoach = requestBodyData.ToObject<Coach>();

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
                                /* PREVIOUSLY: */
                            //command.Parameters.Add("@workload", SqlDbType.Int).Value = newCoach.workload;
                            //command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;
                                /* CHANGED to: due to consistency and scalability */
                            dynamic dObject = newCoach;
                            AddSqlInjection(requestBodyData, dObject, command);

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