using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace TinderCloneV1 {
    public class CoachTutorantController {

        ICoachTutorantService coachTutorantService;
        
        public CoachTutorantController(ICoachTutorantService coachTutorantService) {
            this.coachTutorantService = coachTutorantService;
        }

        [FunctionName("PostCoachTutorant")]
        public async Task<HttpResponseMessage> PostCoachTutorant([HttpTrigger(AuthorizationLevel.Function,
            "post", Route = "coachTutorant")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            coachTutorantService = new CoachTutorantService(req, request, log);

            return await coachTutorantService.CreateConnection();

        }

        [FunctionName("CoachConnectionTutorantByID")]
        public async Task<HttpResponseMessage> GetCoachTutorantCoach([HttpTrigger(AuthorizationLevel.Function,
            "get", "delete", Route = "coachTutorant/coach/{studentID}")] HttpRequestMessage req, HttpRequest request, int studentID, ILogger log) {

            coachTutorantService = new CoachTutorantService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachTutorantService.GetAllCoachConnections(studentID);
            }
            else if (req.Method == HttpMethod.Delete) {
                return await coachTutorantService.DeleteCoachConnection(studentID);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("TutorantConnectionCoachByID")]
        public async Task<HttpResponseMessage> GetCoachTutorantTutorant([HttpTrigger(AuthorizationLevel.Function,
            "get", "put", "delete", Route = "coachTutorant/tutorant/{studentID}")] HttpRequestMessage req, HttpRequest request, int studentID, ILogger log) {

            coachTutorantService = new CoachTutorantService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachTutorantService.GetTutorantConnectionByID(studentID);
            } 
            else if (req.Method == HttpMethod.Put) {
                return await coachTutorantService.UpdateConnectionByID(studentID);
            }
            else if (req.Method == HttpMethod.Delete) {
                return await coachTutorantService.DeleteTutorantConnection(studentID);
            }
            else {
                throw new NotImplementedException();
            }
        }
    }
}
