/*
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Reflection;

namespace TinderCloneV1
{
    public static class UsersController
    {
        [FunctionName("getUsers")]
        [Obsolete]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, TraceWriter log)
        {
            ExceptionHandler exception = new ExceptionHandler(0);

            string str = Environment.GetEnvironmentVariable("sqldb_connection");
            List<User> listOfUsers = new List<User>();
            string queryString = null;
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
                    log.Info(e.Message);
                    return exception.ServiceUnavailable(log);
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

                    log.Info($"Executing the following query: {queryString}");

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
                    log.Info($"{HttpStatusCode.OK} | Data shown succesfully");

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                    };

                }
                catch (SqlException e)
                {
                    log.Info(e.StackTrace);
                    return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
                }
            }
        }
    }
}
*/
