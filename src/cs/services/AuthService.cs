using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Data.SqlClient;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1
{
    class AuthService : IAuthService
    {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly ILogger log;

        public AuthService(HttpRequestMessage req, HttpRequest request, ILogger log)
        {
            this.req = req;
            this.log = log;
        }

        /*Returns */
        public async Task<HttpResponseMessage> CreateAuth() {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
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
             * Check if the ID is already in the db
             * Check password length and stuff
             * Encrypt password (make function for)
             * ********************************* */
            //encrypt password
            userAuth = encryptPassword(userAuth);

            /* Create query for setting the data into the database */
            string queryString = $@"INSERT INTO [dbo].[Auth] (studentID, part1, part2)
                                    VALUES (@studentID, @salt, @hash);";

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
                            command.Parameters.Add("@salt", System.Data.SqlDbType.VarChar).Value = userAuth.salt;
                            command.Parameters.Add("@hash", System.Data.SqlDbType.VarChar).Value = userAuth.hash;

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
            try
            {
                response.Content = new StringContent(leaseToken(userAuth.studentID.ToString()));
            }
            catch (Exception e)
            {
                log.LogError("Somthing went wrong within the token system");
                log.LogError(e.Message);
            }
            return response;
        }


        /*Returns */
        public async Task<HttpResponseMessage> Login()
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
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

            /* Create query for selecting data from the database */
            string queryString = $@"SELECT part1, part2 FROM [dbo].[Auth] where studentID = @studentID";

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

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                /* If the student does not exist, it returns a notFoundException */
                                /* Return status code 404 */
                                while (reader.Read())
                                {
                                    //part1 = salt, part2 = hash
                                    userAuth.salt = reader.GetString(0);
                                    userAuth.hash = reader.GetString(1);
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

            log.LogInformation($"{HttpStatusCode.Created} | Connection created succesfully.");
            HttpResponseMessage response;

            if (userAuth.hash == encryptPassword(userAuth).hash)
            {
                //Return response code [201 Created].
                response = new HttpResponseMessage(HttpStatusCode.OK);
                try
                {
                    response.Content = new StringContent(leaseToken(userAuth.studentID.ToString()));
                }
                catch (Exception e)
                {
                    log.LogError("Somthing went wrong within the token system");
                    log.LogError(e.Message);
                }
                return response;
            }
            //Return response code [400 BadRequest].
            response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            log.LogInformation("test4");
            return response;
        }

        /*Returns */
        public async Task<HttpResponseMessage> TestToken()
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);
            JObject jObject;
            Tokens token;

            /* Read from the requestBody */
            using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync()))
            {
                jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                token = jObject.ToObject<Tokens>();
            }

            /* Verify if all parameters for the Auth table exist.
            One or more parameters may be missing, in which case a [400 Bad Request] is returned. */
            if (token.token == null)
            {
                log.LogError("Requestbody is missing data for the Auth table!");
                return exceptionHandler.BadRequest(log);
            }

            HttpResponseMessage response;
            if (checkTokenValid(token.token))
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
        private UserAuth encryptPassword(UserAuth user)
        {
            /* ******** To do ******************
             * encrypt password
             * ********************************* */
            //https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
            //https://stackoverflow.com/questions/420843/how-does-password-salt-help-against-a-rainbow-table-attack
            //https://stackoverflow.com/questions/1054022/best-way-to-store-password-in-database
            /*
             * //There is no need of a hash already inside here!
             * user.hash = empty;
             *  
             * if (user.salt == empty) {
             *      user.salt = createSalt();
             *  }
             * user.hash = createHash(user.salt, user.password)
             * user.hash = shuffleHash(user.hash, user.salt, user.password)
                   func: shuffle hash -> 
                        //take something from the salt, encrypt it, put it in the middle of the hash
             */

            //for fixing code, now the password == hash
            user.hash = user.password;
            //for fixing code, now the salt is static, should be user different.
            user.salt = "not a real salt";
            return user;
        }
        private string leaseToken(string studentID)
        {
            // to do: check if there is an old token.
            // to do: make token specific on ip // session // mac adres???
            Tokens oldToken = getOldToken(studentID);
            //bool deletedToken = false;

            if (oldToken != null && checkTokenExpired(oldToken))
            {
                // to do: delete on specific token, instead of a new lookup.
                deleteToken(oldToken);
                //deletedToken = true;
                oldToken = null;
            }
            //check if the token is really null
            if (oldToken == null)
            {
                string newToken = createNewToken();

                postToken(studentID, newToken);

                return newToken;
            }
            return oldToken.token;
        }
        public bool checkTokenValid(string givenToken)
        {
            //use this inside a API call to check if token is valid.
            Tokens Token = getToken(givenToken);
            if (checkTokenExpired(Token))
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
                return false;
            }
            return true;
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
                                        token = reader.GetString(0),
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
                                        token = reader.GetString(0),
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
        private void deleteToken(Tokens oldToken)
        {
            //delete token
            string queryDelete = $@"DELETE token
                                    FROM [dbo].[Tokens]
                                    WHERE token = @Token";
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
                            command.Parameters.Add("@Token", System.Data.SqlDbType.VarChar).Value = oldToken.token;

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