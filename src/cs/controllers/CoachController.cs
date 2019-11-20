using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        /*
        Route to /api/profile/coach
        GET: Gets all the coach profiles (Student and Coach table data)
        POST: Creates a new Coach profile
        */
        [FunctionName ("CoachProfile")]
        public async Task<HttpResponseMessage> CoachProfiles ([HttpTrigger (AuthorizationLevel.Anonymous,
            "get", "post", Route = "profile/coach")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            ExceptionHandler exceptionHandler = new ExceptionHandler (0);

            coachService = new CoachService (req, request, log);

            // List<CoachProfile> listOfCoachProfiles = new List<CoachProfile>();

            if (req.Method == HttpMethod.Get) {
                string listOfCoachProfiles = await coachService.GetAllCoachProfiles();

                if (listOfCoachProfiles.Any()) {
                    return new HttpRequestMessage (HttpStatusCode.OK) {
                        Content = new StringContent (listOfCoachProfiles, Encoding.UTF8, "application/json")
                    };
                } else {
                    return new HttpRequestMessage (HttpStatusCode.NotFound);
                }
                // return await coachService.GetAllCoachProfiles();
            } else if (req.Method == HttpMethod.Post) {
                return await coachService.CreateCoachProfile ();
            } else {
                throw new NotImplementedException ();
            }
        }

        /*
        Route to /api/profile/coach/{coachID}
        WHERE coachID is the studentID of the coach as path parameter
        GET: Gets the specific coach profile data given by the studentID (Student and Coach table data)
        POST: Deletes the coach profile given by the studentID (deletes student and Coach table)
        */
        [FunctionName ("CoachProfileByID")]
        public async Task<HttpResponseMessage> CoachProfile ([HttpTrigger (AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            ExceptionHandler exceptionHandler = new ExceptionHandler (coachID);

            coachService = new CoachService (req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachProfileByID (coachID);
            } else if (req.Method == HttpMethod.Delete) {
                return await coachService.DeleteCoachProfileByID (coachID);
            } else {
                throw new NotImplementedException ();
            }
        }
        /*
        Route to /api/coach/{coachID}
        WHERE coachID is the studentID of the coach as path parameter
        GET: Gets the data only from the coach table given by the studentID
        POST: Updates the data in the coach table given by the studentID. This is to update the workload.
        */
        [FunctionName ("CoachByID")]
        public async Task<HttpResponseMessage> Coach ([HttpTrigger (AuthorizationLevel.Anonymous,
            "get", "put", Route = "coach/{coachID}")] HttpRequestMessage req, HttpRequest request, int coachID, ILogger log) {

            ExceptionHandler exceptionHandler = new ExceptionHandler (coachID);

            coachService = new CoachService (req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await coachService.GetCoachByID (coachID);
            } else if (req.Method == HttpMethod.Put) {
                return await coachService.UpdateCoachByID (coachID);
            } else {
                throw new NotImplementedException ();
            }
        }
    }
}