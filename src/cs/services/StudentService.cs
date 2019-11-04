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
    public class StudentService : IStudentService {

        private readonly string str = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public StudentService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        /*
        Returns the data from all the students that were created (Coaches and Tutorants) 
        based on the filters given by the user through query parameters.
        */
        public async Task<HttpResponseMessage> GetAllStudents() {

            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            List<Student> listofStudents = new List<Student>();
            PropertyInfo[] properties = typeof(Student).GetProperties();

            string queryString = $"SELECT * FROM [dbo].[Student]";

            /*
            If there are any query parameters, loop through the properties of the User 
            to check if they exist, if so, add the given property with its query value 
            to the queryString. This enables filtering between individual words in
            the interests and study columns
            */
            string isEmpty = null;
            if (request.Query.Count != 0) {
                queryString += $" WHERE";
                
                foreach (PropertyInfo p in properties) {
                    if (request.Query[p.Name] != isEmpty) {
                        if (p.Name == "interests" || p.Name == "study") {
                            queryString += $" {p.Name} LIKE '%{request.Query[p.Name]}' AND";
                        } else {
                            queryString += $" {p.Name} = '{request.Query[p.Name]}' AND";
                        }
                    }
                }
                /* Remove ' AND' from the queryString to ensure this is the end of the filtering */
                queryString = queryString.Remove(queryString.Length - 4);
            }

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    /* 
                    The connection is automatically closed when going out of scope of the using block.
                    The connection may fail to open, in which case a [503 Service Unavailable] is returned. 
                    */
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");
                            /* 
                            Executing the queryString to get all Student profiles 
                            and add the data of all students to a list of students 
                            */
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                while (reader.Read()) {
                                    listofStudents.Add(new Student {
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
                        log.LogError("Could not perform given query on the database");
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
            var jsonToReturn = JsonConvert.SerializeObject(listofStudents);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            /* Return the JSON */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /*
        Returns the data from a specific student (Coaches and Tutorants) 
        given by the studentID in the path.
        */
        public async Task<HttpResponseMessage> GetStudentByID(int studentID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(studentID);
            Student newStudent = new Student();

            /* Initialize the queryString */
            string queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = @studentID;";

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
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
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                /* If the student does not exist, it returns a notFoundException */
                                if (!reader.HasRows) {
                                    return exceptionHandler.NotFoundException(log);
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

            /* Return the JSON */
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        /* Update the data from the student given by a requestBody */
        public async Task<HttpResponseMessage> UpdateStudentByID(int studentID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(studentID);
            Student newUser;
            JObject jObject = new JObject();
            PropertyInfo[] properties = typeof(Student).GetProperties();
            
            /* Read the requestBody and put the response into a jObject which can be read later
            Also make a new user object and store the data in the user*/
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                newUser = jObject.ToObject<Student>();
            }

            /* If the responseBody is empty (no data has been given by the user)
            return a BadRequest to say that the User must fill the requestBody.*/
            if (jObject.Properties() == null) {
                log.LogError($"Requestbody is missing data for the student table! Cant change {studentID}");
                return exceptionHandler.BadRequest(log);
            }

            string queryString = $"UPDATE [dbo].[Student] SET ";

            int i = 0;
            /* Loop through the properties of the jObject Object which contains the values given in the requestBody
            fill the queryString with the property names from the User and their corresponding values*/
            foreach (JProperty property in jObject.Properties()) {
                /* ProperyInfo[] properties used here to prevent SQLIjection in the columns */
                queryString += $"{properties[i].Name} = '@{property.Name}',";
                i++;
            }

            /* Remove the last character from the queryString, which is ','  
            And add the WHERE statement*/
            queryString = queryString.Remove(queryString.Length - 1);
            queryString += $" WHERE studentID = @studentID;";


            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    /* 
                    The connection is automatically closed when going out of scope of the using block.
                    The connection may fail to open, in which case a [503 Service Unavailable] is returned. 
                    */
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            /* Parameters are used to prevent SQLInjection */
                            command.Parameters.Add("studentID", System.Data.SqlDbType.Int).Value = newUser.studentID;
                            if (jObject["firstName"] != null) command.Parameters.Add("@firstName", System.Data.SqlDbType.VarChar).Value = newUser.firstName;
                            if (jObject["surName"] != null) command.Parameters.Add("@surName", System.Data.SqlDbType.VarChar).Value = newUser.surName;
                            if (jObject["phoneNumber"] != null) command.Parameters.Add("@phoneNumber", System.Data.SqlDbType.VarChar).Value = newUser.phoneNumber;
                            if (jObject["photo"] != null) command.Parameters.Add("@photo", System.Data.SqlDbType.VarChar).Value = newUser.photo;
                            if (jObject["description"] != null) command.Parameters.Add("@description", System.Data.SqlDbType.VarChar).Value = newUser.description;
                            if (jObject["degree"] != null) command.Parameters.Add("@degree", System.Data.SqlDbType.VarChar).Value = newUser.degree;
                            if (jObject["study"] != null) command.Parameters.Add("@study", System.Data.SqlDbType.VarChar).Value = newUser.study;
                            if (jObject["studyYear"] != null) command.Parameters.Add("@studyYear", System.Data.SqlDbType.Int).Value = newUser.studyYear;
                            if (jObject["interests"] != null) command.Parameters.Add("@interests", System.Data.SqlDbType.VarChar).Value = newUser.interests;

                            log.LogInformation($"Executing the following query: {queryString}");

                            SqlCommand commandUpdate = new SqlCommand(queryString, connection);

                            int affectedRows = commandUpdate.ExecuteNonQuery();

                            /* The SQL query must have been incorrect if no rows were executed, return a [404 Not Found] */
                            if (affectedRows == 0) {
                                log.LogError($"No data has been changed for student: {studentID}");
                                return exceptionHandler.NotFoundException(log);
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
    }
}