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
    class MessageService : IMessageService {

        private readonly string str = Environment.GetEnvironmentVariable("sqldb_connection");

        private ExceptionHandler exceptionHandler;

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;
        private string queryString = null;

        public MessageService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateMessage() {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> DeleteMessageByID(int messageID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            string queryString = $"DELETE FROM [dbo].[Message] WHERE messageID = {messageID}";

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    try {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            log.LogInformation($"Executing the following query: {queryString}");
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException e) {
                        //Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                }
            }
            catch (SqlException e) {
                //Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }
            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");
            //Return response code 204
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Get all messages between a coach and a tutorant (a conversation).
        public async Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID) {
            exceptionHandler = new ExceptionHandler(0);

            List<Message> listOfMessages = new List<Message>();

            // Get a conversation.
            // Either the senderID is that of the coachID and the receiverID is that of the tutorantID
            // or
            // the senderID is that of the tutorantID and the receiverID is that of the coachID.
            queryString = $"SELECT * FROM [dbo].[Conversation] WHERE" +
                "(senderID = {coachID} AND receiverID = {tutorantID}) OR +" +
                "(senderID = {tutorantID} AND receiverID = {coachID})";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    try {
                        connection.Open();
                    }
                    catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                    using (SqlCommand command = new SqlCommand(queryString, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                listOfMessages.Add(new Message
                                {
                                    messageID = reader.GetInt32(0),
                                    type = reader.GetString(2),
                                    payload = reader.GetString(3),
                                    created = reader.GetDateTime(4),
                                    lastModified = reader.GetDateTime(5)
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfMessages);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> GetMessageByID(int messageID) {
            exceptionHandler = new ExceptionHandler(messageID);

            Message newMessage = new Message();
            queryString = $"SELECT * FROM [dbo].[Message] WHERE messageID = {messageID};";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(str)) {
                    try {
                        connection.Open();
                    }
                    catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                    using (SqlCommand command = new SqlCommand(queryString, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            if (!reader.HasRows) {
                                return exceptionHandler.NotFoundException(log);
                            }
                            else {
                                while (reader.Read()) {
                                    newMessage = new Message
                                    {
                                        messageID = reader.GetInt32(0),
                                        type = reader.GetString(2),
                                        payload = reader.GetString(3),
                                        created = reader.GetDateTime(4),
                                        lastModified = reader.GetDateTime(5)
                                    };
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(newMessage);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> UpdateMessageByID(int messageID) {
            exceptionHandler = new ExceptionHandler(messageID);

            Message newMessage;
            string body = await req.Content.ReadAsStringAsync();
            JObject jObject = new JObject();

            using (StringReader reader = new StringReader(body)) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                newMessage = jObject.ToObject<Message>();
            }

            // if data to update is given in the request body.
            if (jObject.Properties() != null) {
                queryString = $"UPDATE [dbo].[Message] SET ";

                foreach (JProperty property in jObject.Properties()) {
                    queryString += $"{property.Name} = '{property.Value}',";
                }

                queryString = queryString.Remove(queryString.Length - 1);
                queryString += $" WHERE messageID = {messageID};";

                log.LogInformation($"Executing the following query: {queryString}");

                try {
                    using (SqlConnection connection = new SqlConnection(str)) {
                        try {
                            connection.Open();
                        }
                        catch (SqlException e) {
                            log.LogError(e.Message);
                            return exceptionHandler.ServiceUnavailable(log);
                        }

                        SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                        await commandUpdate.ExecuteNonQueryAsync();

                        connection.Close();
                    }
                }
                catch (SqlException e) {
                    log.LogError(e.Message);
                    return exceptionHandler.BadRequest(log);
                }
                log.LogInformation($"Changed data of message: {messageID}");
            }
            // No data to update given in the request body.
            else {
                log.LogError($"Request body was empty nothing to change for message: {messageID}");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"Changed data of message: {messageID}")
            };
        }
    }
}
