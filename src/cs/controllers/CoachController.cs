using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace TinderCloneV1 {
    public class CoachController{

        ICoachService coachService;

        public CoachController (ICoachService coachService) {
            this.coachService = coachService;
        }

        [FunctionName("CoachProfile")]
        public async Task<HttpResponseMessage> GetCoaches([HttpTrigger(AuthorizationLevel.Function,
            "get", "post", Route = "profile/coach")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetAllCoaches();
            }
            else if(req.Method == HttpMethod.Post){
                return await coachService.CreateCoach();
            } 
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("CoachProfileByID")]
        public async Task<HttpResponseMessage> GetCoachProfile([HttpTrigger(AuthorizationLevel.Function,
            "get", "delete", Route = "profile/coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachByID(coachID);
            } 
            else if (req.Method == HttpMethod.Delete) {
                return await coachService.DeleteCoachByID(coachID);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("CoachProfileAndWorkloadByID")]
        public async Task<HttpResponseMessage> GetCoach([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", Route = "coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachAndWorkloadByID(coachID);
            } 
            else if (req.Method == HttpMethod.Post) {
                return await coachService.UpdateCoachAndWorkloadByID(coachID);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
