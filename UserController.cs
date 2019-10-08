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
using com.sun.org.apache.bcel.@internal.generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

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
                        User getStudent = new User();
                        queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = {studentID};";

                        using (SqlCommand command = new SqlCommand(queryString, connection)){
                            using (SqlDataReader reader = command.ExecuteReader())  {
                                while (reader.Read())  {
                                   getStudent = new User  {
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
                        var jsonToReturn = JsonConvert.SerializeObject(getStudent);
                        log.Info($"{HttpStatusCode.OK} | Data shown succesfully");

                        return new HttpResponseMessage(HttpStatusCode.OK){
                            Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                        };
                    } 
                    else if (req.Method == HttpMethod.Put) {
                        string body = await req.Content.ReadAsStringAsync();
                        //User newDataStudent = new User();
                        JObject jObject = new JObject();
                        List<String> propertyNames = new List<String>();

                        using (StringReader reader = new StringReader(body)) {
                            string json = reader.ReadToEnd();
                            jObject = JsonConvert.DeserializeObject<JObject>(json);
                            //newDataStudent = jObject.ToObject<User>();
                        }

                        foreach(JProperty property in jObject.Properties()) {
                            queryString = $"UPDATE [dbo].[Student]" +
                                $"SET {property.Name} = '{property.Value}'" +
                                $"WHERE studentID = {studentID};";

                            SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                            await commandUpdate.ExecuteNonQueryAsync();
                        }
                        
                        /*
                        New querystring for every column you want to change instead of changing multiple column at once
                        This is because we do not know how many columns the user wants to change, hence the foreach loop
                        */

                        /* queryString = $"UPDATE [dbo].[Student] " +
                            $"SET firstName = '', surName = '', phoneNumber = '', photo = '', description = '', degree = '', study = '', studyear = , interests = ''" +
                            $"WHERE studentID = {studentID};"; */

                        
                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" Changed data of student: {studentID}");
                    } 
                    else if (req.Method == HttpMethod.Post) {
                        string body = await req.Content.ReadAsStringAsync();
                        int i = student.Count;

                        using (StringReader reader = new StringReader(body)) {
                            string json = reader.ReadToEnd();
                            student.Add(JsonConvert.DeserializeObject<User>(json));
                        }

                        queryString = $"INSERT INTO [dbo].[Student] " +
                            $"(studentID, firstName, surName, phoneNumber, photo, description, degree, study, studyYear, interests) " +
                            $"VALUES " +
                            $"({studentID}, '{student[i].firstName}', '{student[i].surName}', '{student[i].phoneNumber}', '{student[i].photo}', " +
                            $"'{student[i].description}', '{student[i].degree}', '{student[i].study}', {student[i].studyYear}, '{student[i].interests}');";

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" {studentID} Added to the database");
                    } 
                    else if (req.Method == HttpMethod.Delete){
                        queryString = $"DELETE FROM [dbo].[Student] WHERE {studentID} = studentID;";

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $"{studentID} Deleted from the database");
                    } 
                    else {
                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.NotFound, $"HTTP Trigger not initialized, Make sure to use GET, POST, DELETE, PUT");
                    }

                    return HttpResponseMessage;
                }
            } catch (SqlException e){
                log.Info(e.Message);
                return req.CreateResponse($"HttpStatusCode.BadRequest, The following SqlException happened: {e.Message}");
            }
        }
    }
}
