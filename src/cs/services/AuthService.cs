﻿using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    class AuthService : IAuthService {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public AuthService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        /*Returns */
        public async Task<HttpResponseMessage> CreateAuth() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            UserAuth userAuth;
            JObject jObject;

            /* Read from the requestBody */
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync()))
            {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                userAuth = jObject.ToObject<UserAuth>();
            }

            /* Verify if all parameters for the Auth table exist.
            One or more parameters may be missing, in which case a [400 Bad Request] is returned. */
            if (jObject["studentID"] == null || jObject["password"] == null) {
                log.LogError("Requestbody is missing data for the Auth table!");
                return exceptionHandler.BadRequest(log);
            }

            /* ******** To do ******************
             * Check if the ID is already in the db
             * Check password length and stuff
             * Encrypt password (make function for)
             * ********************************* */
            //encrypt password
            string encryptedPassword = encryptPassword(jObject["password"].ToString());

            /* Create query for setting the data into the database */
            string queryString = $@"INSERT INTO [dbo].[Auth] (studentID, password)
                                    VALUES (@studentID, @password);";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = userAuth.studentID;
                            command.Parameters.Add("@password", System.Data.SqlDbType.VarChar).Value = encryptedPassword;

                            log.LogInformation($"Executing the following query: {queryString}");

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Connection created succesfully.");

            //Return response code [201 Created].
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(giveToken(jObject["studentID"].ToString()));
            return response;
        }


        /*Returns */
        public async Task<HttpResponseMessage> Login()
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            UserAuth userAuth;
            JObject jObject;

            /* Read from the requestBody */
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync()))
            {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                userAuth = jObject.ToObject<UserAuth>();
            }

            /* Verify if all parameters for the Auth table exist.
            One or more parameters may be missing, in which case a [400 Bad Request] is returned. */
            if (jObject["studentID"] == null || jObject["password"] == null)
            {
                log.LogError("Requestbody is missing data for the Auth table!");
                return exceptionHandler.BadRequest(log);
            }

            //encrypt password
            string encryptedPassword = encryptPassword(jObject["password"].ToString());

            /* Create query for selecting data from the database */
            string queryString = $@"SELECT password FROM [dbo].[Auth] where studentID = @studentID";

            string databasePassword = null;
            log.LogInformation("test1");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = userAuth.studentID;

                            log.LogInformation($"Executing the following query: {queryString}");
                            log.LogInformation("test2");

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                /* If the student does not exist, it returns a notFoundException */
                                /* Return status code 404 */
                                while (reader.Read())
                                {
                                    databasePassword = reader.GetString(0);
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }
            log.LogInformation("test3");

            log.LogInformation($"{HttpStatusCode.Created} | Connection created succesfully.");
            HttpResponseMessage response;

            if (databasePassword == encryptedPassword)
            {
                //Return response code [201 Created].
                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(giveToken(userAuth.studentID.ToString()));
                return response;
            }
            //Return response code [400 BadRequest].
            response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            return response;
        }

        /*Returns */
        public async Task<HttpResponseMessage> TestToken()
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            UserAuth userAuth;
            JObject jObject;

            /* Read from the requestBody */
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync()))
            {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                userAuth = jObject.ToObject<UserAuth>();
            }

            /* Verify if all parameters for the Auth table exist.
            One or more parameters may be missing, in which case a [400 Bad Request] is returned. */
            if (jObject["token"] == null)
            {
                log.LogError("Requestbody is missing data for the Auth table!");
                return exceptionHandler.BadRequest(log);
            }

            HttpResponseMessage response;
            if (checkTokenValid(jObject["token"].ToString()))
            {
                //Return response code [200 OK].
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Token is valid");
                return response;
            }
            //Return response code [200 OK].
            response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("Token is not valid");
            return response;
        }

        // ************************************ Function, to do split into another class ************************************************
        private string encryptPassword(string password)
        {
            /* ******** To do ******************
             * encrypt password
             * ********************************* */
            return password;
        }
        public string giveToken(string studentID)
        {
            // to do: check if there is an old token.
            // to do: make token specific on ip // session // mac adres???
            Tokens oldToken = getOldToken(studentID);
            if (oldToken == null)
            {
                return "error";
            }

            if (checkTokenExpired(oldToken))
            {
                // to do: delete on specific token, instead of a new lookup.
                deleteToken(studentID);

                string newToken = createNewToken();

                postToken(studentID, newToken);

                return newToken;
            }
            return oldToken.token;
        }
        public bool checkTokenValid(string givenToken)
        {
            Tokens Token = getToken(givenToken);
            if(checkTokenExpired(Token))
            {
                return false;
            }
            return true;
        }
        private string createNewToken()
        {
            // to do: make the token better protected, more randomness and stuff like that.
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            //https://stackoverflow.com/questions/14643735/how-to-generate-a-unique-token-which-expires-after-24-hours
            return token;
        }
        private bool checkTokenExpired(Tokens curToken)
        {
            //check if latest token is still usable
            if ((DateTime.Now - curToken.created_at).TotalHours < 24)
            {
                return true;
            }
            return false;
        }
        private Tokens getToken(String token)
        {
            string queryString = $@"SELECT token, created_at FROM [dbo].[Tokens]
                                    WHERE token = @token;";
            Tokens curToken = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@token", System.Data.SqlDbType.VarChar).Value = token;

                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                /* If the student does not exist, it returns a notFoundException */
                                /* Return status code 404 */
                                while (reader.Read())
                                {
                                    curToken = new Tokens
                                    {
                                        tokenID = reader.GetInt32(0),
                                        created_at = reader.GetDateTime(1)
                                    };
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
            }
            return curToken;
        }
        private Tokens getOldToken(string studentID)
        {
            string queryString = $@"SELECT token, created_at FROM [dbo].[Tokens]
                                    INNER JOIN Auth ON Auth.pwID = Tokens.pwID
                                    WHERE studentID = @studentID";
            Tokens curToken = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = studentID;

                            log.LogInformation($"Executing the following query: {queryString}");

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                /* If the student does not exist, it returns a notFoundException */
                                /* Return status code 404 */
                                while (reader.Read())
                                {
                                    curToken = new Tokens
                                    {
                                        tokenID = reader.GetInt32(0),
                                        created_at = reader.GetDateTime(1)
                                    };
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
            }
            return curToken;
        }
        private void deleteToken(string studentID)
        {
            //delete token
            string queryDelete = $@"DELETE t
                                    FROM Tokens t
                                    INNER JOIN Auth a
                                        ON a.pwID=t.pwID
                                    WHERE studentID = @studentID";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryDelete, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = studentID;

                            log.LogInformation($"Executing the following query: {queryDelete}");

                            command.ExecuteNonQueryAsync();
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
            }
        }
        private void postToken(string studentID, string token)
        {
            //delete token
            string queryDelete = $@"INSERT INTO Tokens (pwID, token)
                                        (SELECT pwID, @token
                                        FROM Auth
                                        WHERE studentID = @studentID
                                            );";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //The connection is automatically closed when going out of scope of the using block.
                    //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                    connection.Open();
                    try
                    {
                        //Update the status for the tutorant/coach connection
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        using (SqlCommand command = new SqlCommand(queryDelete, connection))
                        {
                            //Parameters are used to ensure no SQL injection can take place
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.Int).Value = studentID;
                            command.Parameters.Add("@token", System.Data.SqlDbType.VarChar).Value = token;

                            log.LogInformation($"Executing the following query: {queryDelete}");

                            command.ExecuteNonQueryAsync();
                        }
                    }
                    catch (SqlException e)
                    {
                        //The Query may fail, in which case a [400 Bad Request] is returned.
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                    }
                }
            }
            catch (SqlException e)
            {
                //The connection may fail to open, in which case a [503 Service Unavailable] is returned.
                log.LogError("SQL has failed to open.");
                log.LogError(e.Message);
            }
        }
    }
    
}