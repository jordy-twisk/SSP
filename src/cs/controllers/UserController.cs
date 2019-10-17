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
    public class UserController{


        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exception = new ExceptionHandler(0);
        string queryString = null;

        [FunctionName("GetUsers")]
        public async Task<HttpResponseMessage> GetUsers([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, ILogger log){
            
            UserService userService = new UserService(req, request, log);

            return await userService.GetAll();
        }

        [FunctionName("GetUser")]
        public async Task<HttpResponseMessage> GetUser([HttpTrigger(AuthorizationLevel.Anonymous, 
        "get", "post", "delete", "put", Route = "student/{ID}")] HttpRequestMessage req, HttpRequest request, int ID, ILogger log){

            /* Setup the sql connection string, get the string from the environment */
            
            /* Intialize local variables*/
            UserService userService = new UserService(req, request, log);

            int studentID = ID;
            User newStudent = null;
            bool studentExists = false;
            HttpResponseMessage httpResponseMessage = null;

            using (SqlConnection connection = new SqlConnection(str)) {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.Message);
                    return exception.ServiceUnavailable(log);
                }
                try {
                    if (req.Method == HttpMethod.Get){
                        return await userService.GetStudent(studentID, connection);
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

                            log.LogInformation($"Executing the following query: {queryString}");

                            SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                            await commandUpdate.ExecuteNonQueryAsync();
                        }

                        httpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" Changed data of student: {studentID}");
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

                        log.LogInformation($"Executing the following query: {queryString}");

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        httpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" {studentID} Added to the database");
                    } 
                    else if (req.Method == HttpMethod.Delete){
                        queryString = $"DELETE FROM [dbo].[Student] WHERE {studentID} = studentID;";

                        log.LogInformation($"Executing the following query: {queryString}");

                        SqlCommand command = new SqlCommand(queryString, connection);
                        await command.ExecuteNonQueryAsync();

                        httpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $"{studentID} Deleted from the database");
                    } 
                    else {
                        httpResponseMessage = req.CreateResponse(HttpStatusCode.NotFound, $"HTTP Trigger not initialized, Make sure to use a correct HTTP Trigger");
                    }

                    connection.Close();
                    return httpResponseMessage;
                }
                catch (SqlException e) {
                    log.LogInformation(e.Message);
                    return req.CreateResponse($"HttpStatusCode.BadRequest, The following SqlException happened: {e.Message}");
                }
            } 
        }
    }
}
