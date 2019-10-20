using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1
{
    public class MessageController
    {
        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        Task<HttpResponseMessage> httpResponseMessage = null;

        [FunctionName("GetMessagesByID")]
        public async Task<HttpResponseMessage> GetMessages([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "messages/{coachID}/{tutorantID}")] HttpRequestMessage req, HttpRequest request, int coachID, int tutorantID, ILogger log)
        {

            UserService userService = new UserService(req, request, log);

            using (SqlConnection connection = new SqlConnection(str))
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }
                // [MessagesData]
                // GET all messages between coachID and tutorantID
                httpResponseMessage = null;
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("MessageByID")]
        public async Task<HttpResponseMessage> GetMessage([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", "delete", Route = "message/{messageID}")] HttpRequestMessage req, HttpRequest request, int messageID, ILogger log)
        {

            UserService userService = new UserService(req, request, log);

            using (SqlConnection connection = new SqlConnection(str))
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }
                // [MessageData]
                // GET: message data
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [MessageData]
                //// PUT: change a message
                else if (req.Method == HttpMethod.Post)
                {
                    httpResponseMessage = null;
                }
                //// [MessageData]
                //// DELETE: a message
                else if (req.Method == HttpMethod.Post)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("PostMessage")]
        public async Task<HttpResponseMessage> PostMessage([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "message")] HttpRequestMessage req, HttpRequest request, ILogger log)
        {

            UserService userService = new UserService(req, request, log);

            using (SqlConnection connection = new SqlConnection(str))
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException e)
                {
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }
                // [MessageData]
                // POST: a new message
                httpResponseMessage = null;
                connection.Close();
            }
            return await httpResponseMessage;
        }
    }
}
