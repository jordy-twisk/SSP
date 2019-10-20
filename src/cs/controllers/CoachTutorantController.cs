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
    public class CoachTutorantController
    {
        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        Task<HttpResponseMessage> httpResponseMessage = null;

        [FunctionName("PostCoachTutorant")]
        public async Task<HttpResponseMessage> PostCoachTutorant([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "coachTutorant")] HttpRequestMessage req, HttpRequest request, ILogger log)
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
                //// [CoachTutorantData]
                //// POST: create a new CoachTutorant
                httpResponseMessage = null;
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("CoachConnectionTutorantByID")]
        public async Task<HttpResponseMessage> GetCoachTutorantCoach([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "coachTutorant/coach/{studentID}")] HttpRequestMessage req, HttpRequest request, int studentID, ILogger log)
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
                //// [CoachTutorantData]
                //// GET: all coach connections in the CoachTutorant table by coacheID (studentID)
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [CoachTutorantData]
                //// DELETE: the CoachTutorant table by coacheID (studentID)
                else if (req.Method == HttpMethod.Delete)
                {
                    httpResponseMessage = null;
                }
                connection.Close();
            }
            return await httpResponseMessage;
        }

        [FunctionName("TutorantConnectionCoachByID")]
        public async Task<HttpResponseMessage> GetCoachTutorantTutorant([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", "delete", Route = "coachTutorant/tutorant/{studentID}")] HttpRequestMessage req, HttpRequest request, int studentID, ILogger log)
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
                //// [CoachTutorantData]
                //// GET: all coach connections in the CoachTutorant table by tutorantID (studentID)
                if (req.Method == HttpMethod.Get)
                {
                    httpResponseMessage = null;
                }
                //// [CoachTutorantData]
                //// PUT: the CoachTutorant table by tutorantID (studentID)
                else if (req.Method == HttpMethod.Put)
                {
                    httpResponseMessage = null;
                }
                //// [CoachTutorantData]
                //// DELETE: the CoachTutorant table by tutorantID (studentID)
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
