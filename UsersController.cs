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
using Newtonsoft.Json.Linq;

namespace TinderCloneV1{
    public static class UsersController{
        [FunctionName("getUsers")]
        [Obsolete]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students/search")]HttpRequestMessage req,
        HttpRequest request, TraceWriter log){
            try{
                string str = Environment.GetEnvironmentVariable("sqldb_connection");
                List<User> listOfUsers = new List<User>();
                List<String> listOfParameters = new List<String>();
                // listOfParameters.Add(request.Query)
                string name = request.Query["firstName"];

                foreach (var key in Request.QueryString.AllKeys){

                }

                using (SqlConnection connection = new SqlConnection(str)){

                    connection.Open();

                    string text = $"SELECT * FROM [dbo].[Student] WHERE firstName = '{name}';";

                    using (SqlCommand command = new SqlCommand(text, connection)){
                        using (SqlDataReader reader = command.ExecuteReader()){
                            while (reader.Read()){
                                listOfUsers.Add(new User{
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

                    return new HttpResponseMessage(HttpStatusCode.OK){
                        Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                    };

                }
            }
            catch (SqlException e){
                log.Info(e.StackTrace);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
            }
        }
    }
}
