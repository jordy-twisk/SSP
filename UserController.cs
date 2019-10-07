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
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", "put", Route = "{ID}/profile")]HttpRequestMessage req, 
            HttpRequest request, int ID, TraceWriter log){

            List<User> student = new List<User>();
            HttpResponseMessage HttpResponseMessage = null;
            int studentID = ID;
            string queryString;

            try{
                string str = Environment.GetEnvironmentVariable("sqldb_connection");

                using (SqlConnection connection = new SqlConnection(str)){
                    connection.Open();
                    
                    if (req.Method == HttpMethod.Get){
                        queryString = $"SELECT * FROM [dbo].[Student] WHERE studentNumber = {studentID};";

                        using (SqlCommand command = new SqlCommand(queryString, connection)){
                            using (SqlDataReader reader = command.ExecuteReader()){
                                while (reader.Read()){
                                    student.Add(new User{
                                        studentNumber = reader.GetInt32(0),
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
                        var jsonToReturn = JsonConvert.SerializeObject(student);
                        log.Info($"{HttpStatusCode.OK} | Data shown succesfully");
                        return new HttpResponseMessage(HttpStatusCode.OK){
                            Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                        };
                    }

                    else if (req.Method == HttpMethod.Delete){
                        queryString = $"DELETE FROM [dbo].[Student] WHERE {studentID} = studentNumber;";

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $"{studentID} Deleted from the database");
                    }

                    else if (req.Method == HttpMethod.Post){
                        string body = await req.Content.ReadAsStringAsync();
                        int i = student.Count;

                        using (StringReader reader = new StringReader(body)){
                            string json = reader.ReadToEnd();
                            student.Add(JsonConvert.DeserializeObject<User>(json));
                        }
                        
                        queryString = $"INSERT INTO [dbo].[Student] " +
                            $"(studentNumber, firstName, surName, phoneNumber, photo, description, degree, study, studyYear, interests) " +
                            $"VALUES " +
                            $"({studentID}, '{student[i].firstName}', '{student[i].surName}', '{student[i].phoneNumber}', '{student[i].photo}', " +
                            $"'{student[i].description}', '{student[i].degree}', '{student[i].study}', {student[i].studyYear}, '{student[i].interests}');";

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" {studentID} Added to the database");
                    }

                    else{
                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.NotFound, $"HTTP Trigger not initialized, Make sure to use GET, POST, DELETE, PUT");
                    }

                    return HttpResponseMessage;
                }
            }
            catch (SqlException e){
                log.Info(e.Message);
                return req.CreateResponse($"HttpStatusCode.BadRequest, The following SqlException happened: {e.Message}");
            }
        }
    }
}
