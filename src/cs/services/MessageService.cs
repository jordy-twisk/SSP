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

        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

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
            JObject jObject;

            // Read from the request body.
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                message = jObject.ToObject<Message>();
                log.LogInformation($"{message}");
            }

            // Verify if all parameters for the Message table exist,
            // return response code 400 if one or more of the parameters are missing.
            if (jObject["type"] == null     || jObject["payload"] == null      ||
                jObject["created"] == null  || jObject["lastModified"] == null ||
                jObject["senderID"] == null || jObject["receiverID"] == null)  {
                    log.LogError($"Requestbody is missing data for the Message table!");
                    return exceptionHandler.BadRequest(log);
            }

            // All fields for the Message table are required.
            queryString = $@"INSERT INTO [dbo].[Message] (type, payload, created, lastModified, senderID, receiverID)" +
                $"VALUES (@type, @payload, @created, @lastModified, @senderID, @receiverID);";

            try {
            
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case return a [503 Service Unavailable].
                    connection.Open();
       
                    try {
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
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        // Reasons for this failure may include a PK violation (entering an already existing studentID).
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            }
            catch (SqlException e) {
                // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Message created succesfully.");

            // Return response code [201 Created].
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        public async Task<HttpResponseMessage> DeleteMessageByID(int messageID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            queryString = $@"DELETE FROM [dbo].[Message] WHERE MessageID = @MessageID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@MessageID", System.Data.SqlDbType.DateTime).Value = messageID;
                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = command.ExecuteNonQuery();

                            // The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Tutorant table.");
                                return exceptionHandler.NotFoundException(log);
                            }
                        }
                    }
                    catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            }
            catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");

            // Return response code [204 NoContent].
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
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@coachID", System.Data.SqlDbType.Int).Value = coachID;
                            command.Parameters.Add("@tutorantID", System.Data.SqlDbType.Int).Value = tutorantID;
                            log.LogInformation($"Executing the following query: {queryString}");
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    // Query was succesfully executed, but returned no data.
                                    // Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                } else {
                                    while (reader.Read()) {
                                        listOfMessages.Add(new Message {
                                            MessageID = GeneralFunctions.SafeGetInt(reader, 0),
                                            type = GeneralFunctions.SafeGetString(reader, 1),
                                            payload = GeneralFunctions.SafeGetString(reader, 2),
                                            created = GeneralFunctions.SafeGetDateTime(reader, 3),
                                            lastModified = GeneralFunctions.SafeGetDateTime(reader, 4),
                                            senderID = GeneralFunctions.SafeGetInt(reader, 5),
                                            receiverID = GeneralFunctions.SafeGetInt(reader, 6)
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException e) {
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                        // Return response code 503.
                        return exceptionHandler.ServiceUnavailable(log);
                    }
                }
            }
            catch (SqlException e) {
                // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
                // Return response code 400.
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(listOfMessages);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            // Everything went fine, return status code 200.
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
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@messageID", System.Data.SqlDbType.Int).Value = messageID;
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    //Query was succesfully executed, but returned no data.
                                    //Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        newMessage = new Message {
                                            MessageID = reader.GetInt32(0),
                                            type = GeneralFunctions.SafeGetString(reader, 1),
                                            payload = GeneralFunctions.SafeGetString(reader, 2),
                                            created = GeneralFunctions.SafeGetDateTime(reader, 3),
                                            lastModified = GeneralFunctions.SafeGetDateTime(reader, 4),
                                            senderID = GeneralFunctions.SafeGetInt(reader, 5),
                                            receiverID = GeneralFunctions.SafeGetInt(reader, 6)
                                        };
                                    }
                                }
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(newMessage);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully.");

            //Return response code [200 OK] and the requested data.
            // Everything went fine, return status code 200.
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
                    using (SqlConnection connection = new SqlConnection(connectionString)) {
                        //The connection is automatically closed when going out of scope of the using block.
                        //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                        connection.Open();
                        try {
                            using (SqlCommand command = new SqlCommand(queryString, connection)) {
                                // Parameters are used to ensure no SQL injection can take place.
                                command.Parameters.Add("@messageID", System.Data.SqlDbType.Int).Value = messageID;
                                log.LogInformation($"Executing the following query: {queryString}");

                                int affectedRows = command.ExecuteNonQuery();

                                //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                                if (affectedRows == 0) {
                                    log.LogError("Zero rows were affected.");
                                    return exceptionHandler.NotFoundException(log);
                                }
                            }
                        }
                        catch (SqlException e) {
                            //The Query may fail, in which case a [400 Bad Request] is returned.
                            log.LogError("SQL Query has failed to execute.");
                            log.LogError(e.Message);
                            return exceptionHandler.BadRequest(log);
                        }
                    }
                }
                catch (SqlException e) {
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    log.LogError("SQL has failed to open.");
                    log.LogError(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }
                log.LogInformation($"{HttpStatusCode.NoContent} | Data updated succesfully.");
            }

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
