using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class MessageService : IMessageService {

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public MessageService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateMessage() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteMessageByID(int messageID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetMessageByID(int messageID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> UpdateMessageByID(int messageID) {
            throw new NotImplementedException();
        }
    }
}
