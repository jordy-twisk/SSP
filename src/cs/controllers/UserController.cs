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

namespace TinderCloneV1{
    public static class UserController{
        [FunctionName("UserController")]
        [Obsolete]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, 
        "get", "post", "delete", "put", Route = "student/{ID}")] HttpRequestMessage req, int ID, TraceWriter log){

            /* Setup the sql connection string, get the string from the environment */
            string str = Environment.GetEnvironmentVariable("sqldb_connection");
            
            ExceptionHandler exception = new ExceptionHandler(ID);

            /* Intialize local variables*/
            HttpResponseMessage HttpResponseMessage = null;
            int studentID = ID;
            string queryString = null;
            User newStudent = null;
            bool studentExists = false;

            using (SqlConnection connection = new SqlConnection(str)) {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.Info(e.Message);
                    return exception.ServiceUnavailable(log);
                }
                try {
                    if (req.Method == HttpMethod.Get){
                        queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = {studentID};";

                        log.Info($"Executing the following query: {queryString}");
                        
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                studentExists = reader.HasRows;
                                log.Info($"studentExists: {studentExists}");

                                if (!studentExists){
                                    return exception.NotFoundException(log);
                                }
                                else{
                                    while (reader.Read()){
                                        newStudent = new User{
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
                        log.Info($"{HttpStatusCode.OK} | Data shown succesfully");

                        return new HttpResponseMessage(HttpStatusCode.OK){
                            Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                        };
                    } 
                    else if (req.Method == HttpMethod.Put) {
                        string body = await req.Content.ReadAsStringAsync();
                        JObject jObject = new JObject();
                        List<String> propertyNames = new List<String>();

                        using (StringReader reader = new StringReader(body)) {
                            string json = reader.ReadToEnd();
                            jObject = JsonConvert.DeserializeObject<JObject>(json);
                            newStudent = jObject.ToObject<User>();
                        }

                        /* Properties is only null when the User does not want to change anything, this should never happen in a PUT
                        But it's here anyway for safety*/
                        if(jObject.Properties() != null) {
                            queryString = $"UPDATE [dbo].[Student] SET ";
                            
                            foreach (JProperty property in jObject.Properties()) {
                                queryString += $"{property.Name} = '{property.Value}',";
                            }

                            /*Remove the last ',' to ensure a working query. Add the condition statement to the end*/
                            queryString = queryString.Remove(queryString.Length - 1);
                            queryString += $" WHERE studentID = {studentID};";

                            log.Info($"Executing the following query: {queryString}");

                            SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                            await commandUpdate.ExecuteNonQueryAsync();
                        }
                        
                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" Changed data of student: {studentID}");
                    } 
                    else if (req.Method == HttpMethod.Post) {
                        string body = await req.Content.ReadAsStringAsync();

                        using (StringReader reader = new StringReader(body)) {
                            string json = reader.ReadToEnd();
                            newStudent = JsonConvert.DeserializeObject<User>(json);
                        }

                        queryString = $"INSERT INTO [dbo].[Student] " +
                            $"(studentID, firstName, surName, phoneNumber, photo, description, degree, study, studyYear, interests) " +
                            $"VALUES " +
                            $"({studentID}, '{newStudent.firstName}', '{newStudent.surName}', '{newStudent.phoneNumber}', '{newStudent.photo}', " +
                            $"'{newStudent.description}', '{newStudent.degree}', '{newStudent.study}', {newStudent.studyYear}, '{newStudent.interests}');";

                        log.Info($"Executing the following query: {queryString}");

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" {studentID} Added to the database");
                    } 
                    else if (req.Method == HttpMethod.Delete){
                        queryString = $"DELETE FROM [dbo].[Student] WHERE {studentID} = studentID;";

                        log.Info($"Executing the following query: {queryString}");

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $"{studentID} Deleted from the database");
                    } 
                    else {
                        HttpResponseMessage = req.CreateResponse(HttpStatusCode.NotFound, $"HTTP Trigger not initialized, Make sure to use a correct HTTP Trigger");
                    }

                    connection.Close();
                    return HttpResponseMessage;
                }
                catch (SqlException e) {
                    log.Info(e.Message);
                    return req.CreateResponse($"HttpStatusCode.BadRequest, The following SqlException happened: {e.Message}");
                }
            } 
        }
    }
}
