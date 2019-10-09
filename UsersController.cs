using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using System.Text;
using System.Data.SqlClient;

namespace TinderCloneV1{
    public static class UsersController{
        [FunctionName("getUsers")]
        [Obsolete]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "search/profiles")]HttpRequestMessage req, TraceWriter log){
            try{
                string str = Environment.GetEnvironmentVariable("sqldb_connection");
                User getStudents = new User();

                using (SqlConnection connection = new SqlConnection(str)){

                    connection.Open();
                    string text = "SELECT * FROM Student;";

                    using (SqlCommand command = new SqlCommand(text, connection)){
                        using (SqlDataReader reader = command.ExecuteReader()){
                            while (reader.Read()){
                                getStudents = new User {
                                    studentID = reader.GetInt32(0),
                                    firstName = reader.GetString(1),
                                    surName = reader.GetString(2)
                                };
                            }
                        }
                    }

                    var jsonToReturn = JsonConvert.SerializeObject(getStudents);
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
