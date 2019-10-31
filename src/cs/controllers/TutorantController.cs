using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class TutorantController {

        ITutorantService tutorantService;

        public TutorantController(ITutorantService tutorantService) {
            this.tutorantService = tutorantService;
        }

        [FunctionName("TutorantProfile")]
        public async Task<HttpResponseMessage> GetTutorants([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "post", Route = "profile/tutorant")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            tutorantService = new TutorantService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await tutorantService.GetAllTutorantProfiles();
            }
            else if (req.Method == HttpMethod.Post) {
                return await tutorantService.CreateTutorantProfile();
            }
            else {
                throw new NotImplementedException();
            }
        }

        [FunctionName("TutorantProfileByID")]
        public async Task<HttpResponseMessage> GetTutorantProfile([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/tutorant/{tutorantID}")] HttpRequestMessage req, HttpRequest request, int tutorantID, ILogger log) {

            tutorantService = new TutorantService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await tutorantService.GetTutorantProfileByID(tutorantID);
            } 
            else if (req.Method == HttpMethod.Delete) {
                return await tutorantService.DeleteTutorantProfileByID(tutorantID);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
