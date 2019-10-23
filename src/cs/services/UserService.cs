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
    public class UserService : IUserService {

        private readonly string str = Environment.GetEnvironmentVariable("sqldb_connection");

        private ExceptionHandler exceptionHandler;

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public UserService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        private string queryString = null;
        public async Task<HttpResponseMessage> GetAll() {

            exceptionHandler = new ExceptionHandler(0);

            List<User> listOfUsers = new List<User>();
            PropertyInfo[] properties = typeof(User).GetProperties();

            queryString = $"SELECT * FROM [dbo].[Student]";

            int i = 0;
            string isEmpty = null;
            foreach (PropertyInfo p in properties) {
                if (i == 0) {
                    queryString += $" WHERE";
                }
                if (request.Query[p.Name] != isEmpty) {
                    if (p.Name == "interests" || p.Name == "study") {
                        queryString += $" {p.Name} LIKE '%{request.Query[p.Name]}%' AND";
                    } else {
                        queryString += $" {p.Name} = '{request.Query[p.Name]}' AND";
                    }
                }
                i++;
            }

            queryString = queryString.Remove(queryString.Length - 4);
            queryString += $" ORDER BY studentID;";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    try {
                        connection.Open();
                    } catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                    using (SqlCommand command = new SqlCommand(queryString, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                listOfUsers.Add(new User {
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
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            } catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfUsers);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> GetStudentByID(int userID) {
            int studentID = userID;
            exceptionHandler = new ExceptionHandler(userID);

            User newUser = new User();
            queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = {studentID};";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    try {
                        connection.Open();
                    } catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                    using (SqlCommand command = new SqlCommand(queryString, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            if (!reader.HasRows) {
                                return exceptionHandler.NotFoundException(log);
                            } else {
                                while (reader.Read()) {
                                    newUser = new User {
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
                                    };
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            } catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(newUser);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> UpdateUserByID(int userID) {
            int studentID = userID;
            exceptionHandler = new ExceptionHandler(studentID);

            User newUser;
            string body = await req.Content.ReadAsStringAsync();
            JObject jObject = new JObject();

            using (StringReader reader = new StringReader(body)) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                newUser = jObject.ToObject<User>();
            }

            if (jObject.Properties() != null) {
                queryString = $"UPDATE [dbo].[Student] SET ";

                foreach (JProperty property in jObject.Properties()) {
                    queryString += $"{property.Name} = '{property.Value}',";
                }

                queryString = queryString.Remove(queryString.Length - 1);
                queryString += $" WHERE studentID = {studentID};";

                log.LogInformation($"Executing the following query: {queryString}");

                try {
                    using (SqlConnection connection = new SqlConnection(str)) {
                        try {
                            connection.Open();
                        } catch (SqlException e) {
                            log.LogError(e.Message);
                            return exceptionHandler.ServiceUnavailable(log);
                        }

                        SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                        await commandUpdate.ExecuteNonQueryAsync();

                        connection.Close();
                    }
                } catch (SqlException e) {
                    log.LogError(e.Message);
                    return exceptionHandler.BadRequest(log);
                }
                log.LogInformation($"Changed data of student: {studentID}");
            } else {
                log.LogError($"Request body was empty nothing to change for student: {studentID}");
            }

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent($" Changed data of student: {studentID}")
            };
        }
    }
}