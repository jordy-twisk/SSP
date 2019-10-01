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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace TinderCloneV1{
    public static class UserController{
        [FunctionName("UserController")]
        [Obsolete]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "{ID}/profile")]HttpRequestMessage req, string ID, TraceWriter log){
            List<Person> person = new List<Person>();
            string studentID = ID;
            string queryString;
            try{
                string str = Environment.GetEnvironmentVariable("sqldb_connection");

                using (SqlConnection connection = new SqlConnection(str)){
                    connection.Open();
                    
                    if (req.Method == HttpMethod.Get){
                        log.Info($"This is GET, see: {req.Method}");

                        queryString = $"SELECT * FROM Person WHERE {studentID} = UserID;";

                        using (SqlCommand command = new SqlCommand(queryString, connection)){
                            using (SqlDataReader reader = command.ExecuteReader()){
                                while (reader.Read()){
                                    person.Add(new Person{
                                        UserID = reader.GetInt32(0),
                                        UserName = reader.GetString(1)
                                    });
                                }
                            }
                        }
                        var jsonToReturn = JsonConvert.SerializeObject(person);
                        log.Info(HttpStatusCode.OK + " | Data shown succesfully");
                        return new HttpResponseMessage(HttpStatusCode.OK){
                            Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                        };
                    }else if (req.Method == HttpMethod.Delete){
                        log.Info($"This is DELETE, see: {req.Method}");
                        queryString = "DELETE FROM Person WHERE " + studentID + " = UserID;";

                        using (SqlCommand command = new SqlCommand(queryString, connection)){
                            var rows = await command.ExecuteNonQueryAsync();
                            log.Info($"{rows} rows were created");
                        }
                        return req.CreateResponse(HttpStatusCode.OK, $" {studentID} Deleted from the database");
                    }else{
                        return req.CreateResponse(HttpStatusCode.NotFound, $"Http Call not found in the code, please use GET or DELETE");
                    }
                }
            }
            catch (SqlException e){
                log.Info(e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.Message}");
            }
        }
    }
}
