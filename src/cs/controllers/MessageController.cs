﻿using System.Threading.Tasks;
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
    public class MessageController {

        IMessageService messageService;

        public MessageController (IMessageService messageService) {
            this.messageService = messageService;
        }

        /*
        Route to /api/messages/{coachID}/{tutorantID}
        WHERE coachID is the studentID of the coach and tutorantID is the tutorant
        GET: Gets the conversation (all the messages) between a coach and his/her tutorant or the other way around
        */
        [FunctionName("GetMessagesByID")]
        public async Task<HttpResponseMessage> GetMessages([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "messages/{coachID}/{tutorantID}")]HttpRequestMessage req, int coachID, int tutorantID, ILogger log) {

            messageService = new MessageService(log);

            if (req.Method == HttpMethod.Get) {
                return await messageService.GetAllMessages(coachID, tutorantID);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        /*
        Route to /api/message/{messageID}
        WHERE messageID is the auto incremented ID of the message
        GET: Gets a specific message from a conversation
        PUT: Updates a message
        DELETE: Deletes a message from the conversation
        */
        [FunctionName("MessageByID")]
        public async Task<HttpResponseMessage> GetMessage([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "put", "delete", Route = "message/{messageID}")] HttpRequestMessage req, int messageID, ILogger log) {

            messageService = new MessageService(log);

            if (req.Method == HttpMethod.Get) {
                return await messageService.GetMessageByID(messageID);
            } 
            else if (req.Method == HttpMethod.Put) {
                JObject newMessageProfile = null;

                /* Read from the requestBody */
                using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                    newMessageProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }

                return await messageService.UpdateMessageByID(messageID, newMessageProfile);
            } 
            else if (req.Method == HttpMethod.Delete) {
                return await messageService.DeleteMessageByID(messageID);
            }
            else {
                throw new NotImplementedException();
            }
        }

        /*
        Route to /api/message
        POST: Creates a message which will be sent into the conversation
        */
        [FunctionName("PostMessage")]
        public async Task<HttpResponseMessage> PostMessage([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "message")] HttpRequestMessage req, ILogger log) {

            messageService = new MessageService(log);

            if (req.Method == HttpMethod.Post) {
                JObject newMessageProfile = null;

                /* Read from the requestBody */
                using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync())) {
                    newMessageProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }

                return await messageService.CreateMessage(newMessageProfile);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
