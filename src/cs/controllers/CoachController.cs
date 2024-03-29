﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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
            "get", "post", Route = "profile/coach")] HttpRequestMessage request, ILogger log) {

            coachService = new CoachService(log);

           
            if (request.Method == HttpMethod.Get) {
                return await coachService.GetAllCoachProfiles();
            } 
            else if (request.Method == HttpMethod.Post) {
                JObject newCoachProfile = null;

                /* Read from the requestBody */
                using (StringReader reader = new StringReader(await request.Content.ReadAsStringAsync())) {
                    newCoachProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }
            
                return await coachService.CreateCoachProfile(newCoachProfile);
            } 
            else {
                throw new NotImplementedException();
            }


        }

        /*
        Route to /api/profile/coach/{coachID}
        WHERE coachID is the studentID of the coach as path parameter
        GET: Gets the specific coach profile data given by the studentID (Student and Coach table data)
        DELETE: Deletes the coach profile given by the studentID (deletes student and Coach table)
        */
        [FunctionName ("CoachProfileByID")]
        public async Task<HttpResponseMessage> CoachProfile ([HttpTrigger (AuthorizationLevel.Anonymous,
            "get", "delete", Route = "profile/coach/{coachID}")] HttpRequestMessage request, int coachID, ILogger log) {

            coachService = new CoachService (log);

            if (request.Method == HttpMethod.Get) {
                return await coachService.GetCoachProfileByID (coachID);
            } 
            else if (request.Method == HttpMethod.Delete) {
                return await coachService.DeleteCoachProfileByID (coachID);
            }
            else {
                throw new NotImplementedException();
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
            "get", "put", Route = "coach/{coachID}")] HttpRequestMessage request, int coachID, ILogger log) {

            coachService = new CoachService (log);

            if (request.Method == HttpMethod.Get) {
                return await coachService.GetCoachByID (coachID);
            } 
            else if (request.Method == HttpMethod.Put) {
                JObject coachData = null;

                /* Read from the requestBody */
                using(StringReader reader = new StringReader(await request.Content.ReadAsStringAsync())) {
                    coachData = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }

                return await coachService.UpdateCoachByID(coachID, coachData);
            } 
            else {
                throw new NotImplementedException ();
            }
        }
    }
}
