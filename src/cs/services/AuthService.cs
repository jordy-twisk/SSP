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
             * Check password length and stuff
             * Encrypt password (make function for)
             * ********************************* */

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
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.VarChar).Value = userAuth.studentID;
                            command.Parameters.Add("@password", System.Data.SqlDbType.Int).Value = userAuth.password;

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
            return new HttpResponseMessage(HttpStatusCode.Created);
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

            /* ******** To do ******************
             * encrypt password (make function for)
             * ********************************* */

            /* Create query for selecting data from the database */
            string queryString = $@"SELECT password FROM [dbo].[Auth] where studentID = @studentID";

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
                            command.Parameters.Add("@studentID", System.Data.SqlDbType.VarChar).Value = userAuth.studentID;

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
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
    
}