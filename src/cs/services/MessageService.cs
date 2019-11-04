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

        private readonly string environmentString = Environment.GetEnvironmentVariable("sqldb_connection");

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

        // Creates a new message based on data given in the request body.
        public async Task<HttpResponseMessage> CreateMessage() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            Message message;
            JObject jObject = new JObject();

            // Read from the request body.
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                message = jObject.ToObject<Message>();
                log.LogInformation($"{message}");
            }

            // Verify if all parameters for the Message table exist,
            // return response code 400 if one or more of the parameters are missing.
            if (jObject["type"] == null ||
                jObject["payload"] == null ||
                jObject["created"] == null ||
                jObject["lastModified"] == null ||
                jObject["senderID"] == null ||
                jObject["receiverID"] == null) {
                    log.LogError($"Requestbody is missing data for the Message table!");
                    return exceptionHandler.BadRequest(log);
                   
            }

            // All fields for the Message table are required.
            queryString = $@"INSERT INTO [dbo].[Message] (type, payload, created, lastModified, senderID, receiverID)" +
                $"VALUES (@type, @payload, @created, @lastModified, @senderID, @receiverID);";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        // The connection is automatically closed when going out of scope of the using block
                        connection.Open();

                        // Insert new message into the Message table.
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.
                            // command.Parameters.Add("@MessageID", System.Data.SqlDbType.Int).Value = message.MessageID;
                            command.Parameters.Add("@type", System.Data.SqlDbType.VarChar).Value = message.type;
                            command.Parameters.Add("@payload", System.Data.SqlDbType.VarChar).Value = message.payload;
                            command.Parameters.Add("@created", System.Data.SqlDbType.DateTime).Value = message.created;
                            command.Parameters.Add("@lastModified", System.Data.SqlDbType.DateTime).Value = message.lastModified;
                            command.Parameters.Add("@senderID", System.Data.SqlDbType.Int).Value = message.senderID;
                            command.Parameters.Add("@receiverID", System.Data.SqlDbType.Int).Value = message.receiverID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException e) {
                        // Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                // Return response code 400.
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Message created succesfully");

            // Return response code 201.
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        public async Task<HttpResponseMessage> DeleteMessageByID(int messageID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            queryString = $@"DELETE FROM [dbo].[Message] WHERE MessageID = @MessageID";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@MessageID", System.Data.SqlDbType.DateTime).Value = messageID;
                            log.LogInformation($"Executing the following query: {queryString}");
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException e) {
                        // Return response code 503
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                }
            }
            catch (SqlException e) {
                // Return response code 400
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }
            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");
            // Return response code 204
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Get all messages between a coach and a tutorant (a conversation between a coach and a tutorant).
        public async Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID) {
            exceptionHandler = new ExceptionHandler(0);

            List<Message> listOfMessages = new List<Message>();

            // Get a conversation.
            // Either the senderID is that of the coachID and the receiverID is that of the tutorantID
            // or
            // the senderID is that of the tutorantID and the receiverID is that of the coachID.

            queryString = $@"SELECT * FROM [dbo].[Message]
                            WHERE (senderID = @coachID AND receiverID = @tutorantID) OR  
                            (senderID = @tutorantID AND receiverID = @coachID);";

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    // Return response code 404
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        listOfMessages.Add(new Message {
                                            MessageID = reader.GetInt32(0),
                                            type = reader.GetString(1),
                                            payload = reader.GetString(2),
                                            created = reader.GetDateTime(3),
                                            lastModified = reader.GetDateTime(4),
                                            senderID = reader.GetInt32(5),
                                            receiverID = reader.GetInt32(6)
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfMessages);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> GetMessageByID(int messageID) {
            exceptionHandler = new ExceptionHandler(messageID);

            Message newMessage = new Message();
            queryString = $@"SELECT * FROM [dbo].[Message] WHERE MessageID = @messageID;";

            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        connection.Open();
                    }
                    catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    }

                    using (SqlCommand command = new SqlCommand(queryString, connection)) {
                        command.Parameters.Add("@messageID", System.Data.SqlDbType.Int).Value = messageID;
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            if (!reader.HasRows) {
                                return exceptionHandler.NotFoundException(log);
                            }
                            else {
                                while (reader.Read()) {
                                    newMessage = new Message
                                    {
                                        MessageID = reader.GetInt64(0),
                                        type = reader.GetString(1),
                                        payload = reader.GetString(2),
                                        created = reader.GetDateTime(3),
                                        lastModified = reader.GetDateTime(4),
                                        senderID = reader.GetInt32(5),
                                        receiverID = reader.GetInt32(6)
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
                queryString += $@" WHERE MessageID = @messageID;";

                log.LogInformation($"Executing the following query: {queryString}");

                try {
                    using (SqlConnection connection = new SqlConnection(environmentString)) {
                        try {
                            // The connection is automatically closed when going out of scope of the using block
                            connection.Open();

                            using (SqlCommand command = new SqlCommand(queryString, connection)) {
                                // Parameters are used to ensure no SQL injection can take place.
                                command.Parameters.Add("@messageID", System.Data.SqlDbType.Int).Value = messageID;
                                log.LogInformation($"Executing the following query: {queryString}");
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (SqlException e) {
                            // Return response code 503
                            log.LogError(e.Message);
                            return exceptionHandler.ServiceUnavailable(log);
                        }
                    }
                }
                catch (SqlException e) {
                    // Return response code 400.
                    log.LogError(e.Message);
                    return exceptionHandler.BadRequest(log);
                }

                log.LogInformation($"{HttpStatusCode.NoContent} | Data updated succesfully");
            }
            // Return response code 204.
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        public string SafeGetString(SqlDataReader reader, int index) {
            if (!reader.IsDBNull(index))
                return reader.GetString(index);
            return string.Empty;
        }

        public int SafeGetInt(SqlDataReader reader, int index) {
            if (!reader.IsDBNull(index))
                return reader.GetInt32(index);
            return 0;
        }
    }
}
