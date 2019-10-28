using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

/*
 * Note:
 * CoachProfile is understood as the combination of the data in the Student table and the Coach table
 * Coach is the understood as the data in the Coach table
 */

namespace TinderCloneV1 {
    public class CoachController {

        ICoachService coachService;

        public CoachController (ICoachService coachService) {
            this.coachService = coachService;
        }

        [FunctionName("CoachProfile")]
        public async Task<HttpResponseMessage> CoachProfiles([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "post", Route = "profile/coach")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetAllCoachProfiles();
            }
            else if(req.Method == HttpMethod.Post){
                return await coachService.CreateCoachProfile();
            } 
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("CoachProfileByID")]
        public async Task<HttpResponseMessage> CoachProfile([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachProfileByID(coachID);
            } 
            else if (req.Method == HttpMethod.Delete) {
                return await coachService.DeleteCoachProfileByID(coachID);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("CoachByID")]
        public async Task<HttpResponseMessage> Coach([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", Route = "coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            coachService = new CoachService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachByID(coachID);
            } 
            else if (req.Method == HttpMethod.Put) {
                return await coachService.UpdateCoachByID(coachID);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
