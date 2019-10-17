using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1
{
    class UserService
    {
        HttpRequestMessage req;
        HttpRequest request;
        ILogger log;
        public UserService(HttpRequestMessage req, HttpRequest request, ILogger log){
            this.req = req;
            this.request = request;
            this.log = log;
        }

        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        List<User> listOfUsers = new List<User>();
        string queryString = null;

        [Obsolete]
        public async Task<HttpResponseMessage> GetAll()
        {

            string isEmpty = null;

            PropertyInfo[] properties = typeof(User).GetProperties();

            using (SqlConnection connection = new SqlConnection(str))
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }

                try
                {
                    queryString = $"SELECT * FROM [dbo].[Student]";
                    int i = 0;

                    foreach (PropertyInfo p in properties)
                    {
                        if (i == 0)
                        {
                            queryString += $" WHERE";
                        }

                        if (request.Query[p.Name] != isEmpty)
                        {
                            if (p.Name == "interests" || p.Name == "study")
                            {
                                queryString += $" {p.Name} LIKE '%{request.Query[p.Name]}%' AND";
                            }
                            else
                            {
                                queryString += $" {p.Name} = '{request.Query[p.Name]}' AND";
                            }
                        }

                        i++;
                    }

                    queryString = queryString.Remove(queryString.Length - 4);
                    queryString += $" ORDER BY studentID;";

                    log.LogInformation($"Executing the following query: {queryString}");

                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listOfUsers.Add(new User
                                {
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

                    var jsonToReturn = JsonConvert.SerializeObject(listOfUsers);
                    log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

                    connection.Close();

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                    };
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.StackTrace);
                    return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
                }
            }
        }

        public async Task<HttpResponseMessage> GetStudent(int ID, SqlConnection con){

            int studentID = ID;
            User newStudent = null;
            bool studentExists = false;
            HttpResponseMessage HttpResponseMessage = null;

            queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = {studentID};";

            log.LogInformation($"Executing the following query: {queryString}");

            using (SqlCommand command = new SqlCommand(queryString, con))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    studentExists = reader.HasRows;

                    if (!studentExists)
                    {
                        return exceptionHandler.NotFoundException(log);
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            newStudent = new User
                            {
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

            var jsonToReturn = JsonConvert.SerializeObject(newStudent);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }
    }
}
