using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace TinderCloneV1 {
    public class CoachTutorantController {

        ICoachTutorantService coachTutorantService;
        
        public CoachTutorantController(ICoachTutorantService coachTutorantService) {
            this.coachTutorantService = coachTutorantService;
        }

        /*
        Route to /api/coachTutorant
        PUT: Updates the status of the connection between the coach and his tutorant. (Pending, Completed etc)
        */
        [FunctionName("PutCoachTutorant")]
        public async Task<HttpResponseMessage> PutCoachTutorant([HttpTrigger(AuthorizationLevel.Anonymous,
            "put", Route = "coachTutorant")] HttpRequestMessage request, ILogger log) {

            coachTutorantService = new CoachTutorantService(log);

            if (request.Method == HttpMethod.Put) {
                JObject coachTutorantConnProfile = null;

                /* Read from the requestBody */
                using (StringReader reader = new StringReader(await request.Content.ReadAsStringAsync())) {
                    coachTutorantConnProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }

                return await coachTutorantService.UpdateConnection(coachTutorantConnProfile);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        /*
        Route to /api/coachTutorant/coach/{studentID}
        WHERE studentID is the studentID of the student (coach) as path parameter
        GET: Gets the connection data from the coach connected with his/her tutorants
        DELETE: Deletes the connections the coach has running with hsi/her tutorants
        */
        [FunctionName("CoachConnectionTutorantByID")]
        public async Task<HttpResponseMessage> CoachTutorantCoach([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "delete", Route = "coachTutorant/coach/{studentID}")] HttpRequestMessage request, int studentID, ILogger log) {

            coachTutorantService = new CoachTutorantService(log);

            if (request.Method == HttpMethod.Get) {
                return await coachTutorantService.GetAllConnectionsByCoachID(studentID);
            }
            else if (request.Method == HttpMethod.Delete) {
                return await coachTutorantService.DeleteConnectionByCoachID(studentID);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        /*
        Route to /api/coachTutorant/tutorant/{studentID}
        WHERE studentID is the studentID of the student (tutorant) as path parameter
        GET: Gets the connection data from the tutorant connected with his/her coach
        POST: Creates (or request) a connection to a coach
        DELETE: Deletes the tutorants connection with his/her coach
        */
        [FunctionName("TutorantConnectionCoachByID")]
        public async Task<HttpResponseMessage> CoachTutorantTutorant([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "post", "delete", Route = "coachTutorant/tutorant/{studentID}")] HttpRequestMessage request, int studentID, ILogger log) {

            coachTutorantService = new CoachTutorantService(log);

            if (request.Method == HttpMethod.Get) {
                return await coachTutorantService.GetConnectionByTutorantID(studentID);
            } 
            else if (request.Method == HttpMethod.Post) {
                JObject coachTutorantConnProfile = null;

                /* Read from the requestBody */
                using (StringReader reader = new StringReader(await request.Content.ReadAsStringAsync())) {
                    coachTutorantConnProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }

                return await coachTutorantService.CreateConnectionByTutorantID(studentID, coachTutorantConnProfile); 
            }
            else if (request.Method == HttpMethod.Delete) {
                return await coachTutorantService.DeleteConnectionByTutorantID(studentID);
            }
            else {
                throw new NotImplementedException();
            }
        }
    }
}
