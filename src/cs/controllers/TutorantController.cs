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
    public class TutorantController
    {
        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        Task<HttpResponseMessage> httpResponseMessage = null;

        [FunctionName("GetTutorants")]
        public async Task<HttpResponseMessage> GetCoaches([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", Route = "profile/tutorant")] HttpRequestMessage req, HttpRequest request, ILogger log)
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
                // [TutorantData]
                // GET all tutorant data
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [TutorantData]
                //// Post: create a new tutorant
                else if (req.Method == HttpMethod.Post)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("GetCoachProfile")]
        public async Task<HttpResponseMessage> GetCoachProfile([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/tutorant/{ID}")] HttpRequestMessage req, HttpRequest request, int ID, ILogger log)
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
                // [TutorantData]
                // GET the tutorant data by studentID
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [TutorantData]
                //// Delete: tutorant by studentID
                else if (req.Method == HttpMethod.Delete)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }
    }
}
