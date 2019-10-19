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

    public interface IUserService {
        Task<HttpResponseMessage> GetAll(SqlConnection con);
        Task<HttpResponseMessage> GetStudent(int ID, SqlConnection con);
        Task<HttpResponseMessage> PutStudent(int ID, SqlConnection con);
    }

    class UserService : IUserService{
        HttpRequestMessage req;
        HttpRequest request;
        ILogger log;
        public UserService(HttpRequestMessage req, HttpRequest request, ILogger log){
            this.req = req;
            this.request = request;
            this.log = log;
        }

        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        List<User> listOfUsers = new List<User>();
        string queryString = null;

        [Obsolete]
        public async Task<HttpResponseMessage> GetAll(SqlConnection con) {

            //log.LogInformation($"con.State from Service: {con.State}");

            string isEmpty = null;

            PropertyInfo[] properties = typeof(User).GetProperties();

            queryString = $"SELECT * FROM [dbo].[Student]";
            int i = 0;

            foreach (PropertyInfo p in properties) {
                if (i == 0) {
                    queryString += $" WHERE";
                }

                if (request.Query[p.Name] != isEmpty) {
                    if (p.Name == "interests" || p.Name == "study") {
                        queryString += $" {p.Name} LIKE '%{request.Query[p.Name]}%' AND";
                    }
                    else {
                        queryString += $" {p.Name} = '{request.Query[p.Name]}' AND";
                    }
                }

                i++;
            }

            queryString = queryString.Remove(queryString.Length - 4);
            queryString += $" ORDER BY studentID;";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlCommand command = new SqlCommand(queryString, con)) {
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

                var jsonToReturn = JsonConvert.SerializeObject(listOfUsers);
                log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

                con.Close();

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                };
            }
            catch (SqlException e) {
                log.LogError(e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
            }
        }

        public async Task<HttpResponseMessage> GetStudent(int ID, SqlConnection con){

            int studentID = ID;
            queryString = $"SELECT * FROM [dbo].[Student] WHERE studentID = {studentID};";

            log.LogInformation($"Executing the following query: {queryString}");

            try{
                using (SqlCommand command = new SqlCommand(queryString, con)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        if (!reader.HasRows)  {
                            return exceptionHandler.NotFoundException(log);
                        }
                        else {
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
                } 
            }
            catch (SqlException e) {
                log.LogError(e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfUsers);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }


        public async Task<HttpResponseMessage> PutStudent(int ID, SqlConnection con){
            int studentID = ID;
            string body = await req.Content.ReadAsStringAsync();
            JObject jObject = new JObject();
            List<String> propertyNames = new List<String>();

            using (StringReader reader = new StringReader(body)) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                listOfUsers.Add(jObject.ToObject<User>());
            }

            if(jObject.Properties() != null) {
                queryString = $"UPDATE [dbo].[Student] SET ";
                
                foreach (JProperty property in jObject.Properties()) {
                    queryString += $"{property.Name} = '{property.Value}',";
                }

                queryString = queryString.Remove(queryString.Length - 1);
                queryString += $" WHERE studentID = {studentID};";

                log.LogInformation($"Executing the following query: {queryString}");

                try{
                    SqlCommand commandUpdate = new SqlCommand(queryString, con);
                    await commandUpdate.ExecuteNonQueryAsync();
                }
                catch (SqlException e) {
                    log.LogError(e.Message);
                    return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {e.StackTrace}");
                }
            }

            log.LogInformation($" Changed data of student: {studentID}");
            
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent($" Changed data of student: {studentID}")
            };
        }
    }
}
