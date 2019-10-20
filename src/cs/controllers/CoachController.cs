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
    public class CoachController
    {
        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        Task<HttpResponseMessage> httpResponseMessage = null;

        [FunctionName("CoachProfile")]
        public async Task<HttpResponseMessage> GetCoaches([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "post", Route = "profile/coach")] HttpRequestMessage req, HttpRequest request, ILogger log)
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
                // [CoachData]
                // GET: all coaches data
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [CoachData]
                //// POST: create a new coach
                else if (req.Method == HttpMethod.Post)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("CoachProfileByID")]
        public async Task<HttpResponseMessage> GetCoachProfile([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/coach/{ID}")] HttpRequestMessage req, HttpRequest request, int ID, ILogger log)
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
                // [CoachData]
                // GET: the coach data by studentID
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [CoachData]
                //// DELETE: coach by studentID
                else if (req.Method == HttpMethod.Delete)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }
        [FunctionName("CoachProfileAndWorkloadByID")]
        public async Task<HttpResponseMessage> GetCoach([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", Route = "coach/{ID}")] HttpRequestMessage req, HttpRequest request, int ID, ILogger log)
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
                // [CoachData]
                // GET: the coach and workload by studentID
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [CoachData]
                //// PUT: coach and workload by studentID
                else if (req.Method == HttpMethod.Put)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }
    }
}
