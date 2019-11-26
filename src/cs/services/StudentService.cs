using System;
using System.Net;
using System.Text;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class StudentService : IStudentService {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly ILogger log;

        public StudentService(ILogger log) {
            this.log = log;
        }

        /* Returns the data from all the students that were created (Coaches and Tutorants) 
           based on the filters given by the user through query parameters. */
        public async Task<HttpResponseMessage> GetAllStudents(List<string> parameters, List<string> propertyNames) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            List<Student> listOfStudents = new List<Student>();

            string queryString = $"SELECT * FROM [dbo].[Student]";

            /* If there are any query parameters, loop through the properties of the User 
               to check if they exist, if so, add the given property with its query value 
               to the queryString. This enables filtering between individual words in
               the interests and study columns */
            if (parameters.Count != 0) {
                queryString += $" WHERE";

                for (int i = 0; i < parameters.Count; ++i) {
                    if (parameters[i] == "interests" || parameters[i] == "study") {
                        queryString += $" {propertyNames[i]} LIKE '%{parameters[i]}' AND";
                    } else {
                        queryString += $" {propertyNames[i]} = '{parameters[i]}' AND";
                    }
               }
                /* Remove ' AND' from the queryString to ensure this is the end of the filtering */
                queryString = RemoveLastCharacters(queryString, 4);
            }

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /* The connection is automatically closed when going out of scope of the using block.
                       The connection may fail to open, in which case a [503 Service Unavailable] is returned.  */
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");
                            /* Executing the queryString to get all Student profiles 
                               and add the data of all students to a list of students */
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                while (reader.Read()) {
                                    listOfStudents.Add(new Student {
                                        studentID = reader.GetInt32(0),
                                        firstName = GeneralFunctions.SafeGetString(reader, 1),
                                        surName = GeneralFunctions.SafeGetString(reader, 2),
                                        phoneNumber = GeneralFunctions.SafeGetString(reader, 3),
                                        photo = GeneralFunctions.SafeGetString(reader, 4),
                                        description = GeneralFunctions.SafeGetString(reader, 5),
                                        degree = GeneralFunctions.SafeGetString(reader, 6),
                                        study = GeneralFunctions.SafeGetString(reader, 7),
                                        studyYear = GeneralFunctions.SafeGetInt(reader, 8),
                                        interests = GeneralFunctions.SafeGetString(reader, 9)
                                    });
                                }
                            }
                        }
                    } 
                    catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned. */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } 
            catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            /* Convert the list of students to a JSON and Log a OK message */
            var jsonToReturn = JsonConvert.SerializeObject(listOfStudents);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            /* Return the JSON. Return status code 200 */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /*
        Returns the data from a specific student (Coaches and Tutorants) 
        given by the studentID in the path.
        */
        public async Task<HttpResponseMessage> GetStudentByID(int studentID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            Student newStudent = new Student();

            /* Initialize the queryString */
            string queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = @studentID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /* 
                    The connection is automatically closed when going out of scope of the using block.
                    The connection may fail to open, in which case a [503 Service Unavailable] is returned. 
                    */
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            /* Adding SQL Injection to the StudentID parameter to prevent SQL attacks */
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = studentID;

                            /* 
                            Executing the queryString to get the student profile 
                            and add the data of the student to a newStudent
                            */
                            log.LogInformation($"Executing the following query: {queryString}");
                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                /* If the student does not exist, it returns a notFoundException */
                                /* Return status code 404 */
                                if (!reader.HasRows) {
                                    return exceptionHandler.NotFound();
                                }
                                while (reader.Read()) {
                                    newStudent = new Student {
                                        studentID = reader.GetInt32(0),
                                        firstName = GeneralFunctions.SafeGetString(reader, 1),
                                        surName = GeneralFunctions.SafeGetString(reader, 2),
                                        phoneNumber = GeneralFunctions.SafeGetString(reader, 3),
                                        photo = GeneralFunctions.SafeGetString(reader, 4),
                                        description = GeneralFunctions.SafeGetString(reader, 5),
                                        degree = GeneralFunctions.SafeGetString(reader, 6),
                                        study = GeneralFunctions.SafeGetString(reader, 7),
                                        studyYear = GeneralFunctions.SafeGetInt(reader, 8),
                                        interests = GeneralFunctions.SafeGetString(reader, 9)
                                    };
                                }
                            }
                        }
                    } 
                    catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned. */
                        log.LogError("Could not perform given query on the database");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } 
            catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            /* Convert the student to a JSON and Log a OK message */
            var jsonToReturn = JsonConvert.SerializeObject(newStudent);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            /* Return the JSON  Return status code 200 */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /* Update the data from the student given by a requestBody */
        public async Task<HttpResponseMessage> UpdateStudentByID(int studentID, JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            
            /* Read the requestBody and put the response into a jObject which can be read later
            Also make a new user object and store the data in the user */


            /* If the responseBody is empty (no data has been given by the user)
            return a BadRequest to say that the User must fill the requestBody.
            Bad request is status code 400 */
            if (requestBodyData["studentID"] == null) {
                log.LogError($"Requestbody contains no studentID");
                return exceptionHandler.BadRequest(log);
            }

            Student newStudent = requestBodyData.ToObject<Student>();

            string queryString = $"UPDATE [dbo].[Student] SET ";

            /* Loop through the properties of the jObject Object which contains the values given in the requestBody
               loop through the hardcoded properties in the Student Entity to check if they correspond with the requestBody 
               to prevent SQL injection. */
            foreach (JProperty property in requestBodyData.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        /* fill the queryString with the property names from the Message and their values */
                        queryString += $"{props.Name} = @{property.Name}, ";
                    }
                }
            }

            /* Remove the last character from the queryString, which is ','  
            And add the WHERE statement*/
            queryString = RemoveLastCharacters(queryString, 2);
            queryString += $" WHERE studentID = @studentID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /* 
                    The connection is automatically closed when going out of scope of the using block.
                    The connection may fail to open, in which case a [503 Service Unavailable] is returned. 
                    */
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            /* Parameters are used to prevent SQLInjection */
                            AddSqlInjection(requestBodyData, newStudent, command);

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            /* The SQL query must have been incorrect if no rows were executed, return a [404 Not Found] */
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
                            }
                        }
                    } catch (SqlException e) {
                        /* The query may fail, in which case a [400 Bad Request] is returned. */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            }
            catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"Changed data of student: {studentID}");
            
            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
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