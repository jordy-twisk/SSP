using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace TinderCloneV1 {
    class MessageService : IMessageService {

        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly ILogger log;

        public MessageService(ILogger log) {
            this.log = log;
        }

        // Creates a new message based on data given in the request body.
        public async Task<HttpResponseMessage> CreateMessage(JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            DatabaseFunctions databaseFunctions = new DatabaseFunctions();

            // Verify if all parameters for the Message table exist,
            // return response code 400 if one or more of the parameters are missing.
            if (requestBodyData["type"] == null     || requestBodyData["payload"] == null      ||
                requestBodyData["created"] == null  || requestBodyData["lastModified"] == null ||
                requestBodyData["senderID"] == null || requestBodyData["receiverID"] == null)  {
                    log.LogError($"Requestbody is missing data for the Message table!");
                    return exceptionHandler.BadRequest(log);
            }

            Message newMessage = requestBodyData.ToObject<Message>();

            // All fields for the Message table are required.
            string queryString = $@"INSERT INTO [dbo].[Message] (type, payload, created, lastModified, senderID, receiverID)" +
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
                            dynamic dObject = newMessage;
                            databaseFunctions.AddSqlInjection(requestBodyData, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString}");

                            await command.ExecuteNonQueryAsync();
                        }
                    } catch (SqlException e) {
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        // Reasons for this failure may include a PK violation (entering an already existing studentID).
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
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
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            string queryString = $@"DELETE FROM [dbo].[Message] WHERE MessageID = @MessageID";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();

                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@MessageID", SqlDbType.DateTime).Value = messageID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();

                            // The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected while deleting from the Tutorant table.");
                                return exceptionHandler.NotFound();
                            }
                        }
                    } catch (SqlException e) {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.NoContent} | Data deleted succesfully");

            // Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        // Get all messages between a coach and a tutorant (a conversation between a coach and a tutorant).
        public async Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            List<Message> listOfMessages = new List<Message>();

            // Get a conversation.
            // Either the senderID is that of the coachID and the receiverID is that of the tutorantID
            // or
            // the senderID is that of the tutorantID and the receiverID is that of the coachID.
            string queryString = $@"SELECT * FROM [dbo].[Message]
                                    WHERE (senderID = @coachID AND receiverID = @tutorantID) OR  
                                          (senderID = @tutorantID AND receiverID = @coachID);";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    // The connection is automatically closed when going out of scope of the using block.
                    // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@coachID", SqlDbType.Int).Value = coachID;
                            command.Parameters.Add("@tutorantID", SqlDbType.Int).Value = tutorantID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    // Query was succesfully executed, but returned no data.
                                    // Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                }
                                while (reader.Read()) {
                                    listOfMessages.Add(new Message {
                                        MessageID = reader.GetInt32(0),
                                        type = SafeReader.SafeGetString(reader, 1),
                                        payload = SafeReader.SafeGetString(reader, 2),
                                        created = SafeReader.SafeGetDateTime(reader, 3),
                                        lastModified = SafeReader.SafeGetDateTime(reader, 4),
                                        senderID = SafeReader.SafeGetInt(reader, 5),
                                        receiverID = SafeReader.SafeGetInt(reader, 6)
                                    });
                                }
                            }
                        }
                    } catch (SqlException e) {
                        // The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                // The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
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
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            Message newMessage = new Message();

            string queryString = $@"SELECT * FROM [dbo].[Message] WHERE MessageID = @messageID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            command.Parameters.Add("@messageID", SqlDbType.Int).Value = messageID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = await command.ExecuteReaderAsync()) {
                                if (!reader.HasRows) {
                                    //Query was succesfully executed, but returned no data.
                                    //Return response code [404 Not Found]
                                    log.LogError("SQL Query was succesfully executed, but returned no data.");
                                    return exceptionHandler.NotFound();
                                } 
                                while (reader.Read()) {
                                    newMessage = new Message {
                                        MessageID = reader.GetInt32(0),
                                        type = SafeReader.SafeGetString(reader, 1),
                                        payload = SafeReader.SafeGetString(reader, 2),
                                        created = SafeReader.SafeGetDateTime(reader, 3),
                                        lastModified = SafeReader.SafeGetDateTime(reader, 4),
                                        senderID = SafeReader.SafeGetInt(reader, 5),
                                        receiverID = SafeReader.SafeGetInt(reader, 6)
                                    };
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
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public async Task<HttpResponseMessage> UpdateMessageByID(int messageID, JObject requestBodyData) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            DatabaseFunctions databaseFunctions = new DatabaseFunctions();

            Message newMessage = requestBodyData.ToObject<Message>();

            string queryString = $"UPDATE [dbo].[Message] SET ";

            /* Loop through the properties of the jObject Object which contains the values given in the requestBody
               loop through the hardcoded properties in the Message Entity to check if they correspond with the requestBody 
               to prevent SQL injection. */
            foreach (JProperty property in requestBodyData.Properties()) {
                foreach (PropertyInfo props in newMessage.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        /* fill the queryString with the property names from the Message and their values */
                        queryString += $"{props.Name} = @{property.Name},";
                    }
                }
            }

            queryString = databaseFunctions.RemoveLastCharacters(queryString, 1);
            queryString += $@" WHERE MessageID = @messageID;";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try {
                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            // Parameters are used to ensure no SQL injection can take place.

                            /* pass the requestBody, the entity with the corresponding properties and the SqlCommand to the method 
                               to ensure working SqlInjection for the incoming values*/
                            databaseFunctions.AddSqlInjection(requestBodyData, newMessage, command);

                            log.LogInformation($"Executing the following query: {queryString}");

                            int affectedRows = await command.ExecuteNonQueryAsync();
                            //The SQL query must have been incorrect if no rows were executed, return a [404 Not Found].
                            if (affectedRows == 0) {
                                log.LogError("Zero rows were affected.");
                                return exceptionHandler.NotFound();
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

            //Return response code [204 NoContent].
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
