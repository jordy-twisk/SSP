using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class CoachTutorantService : ICoachTutorantService {

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachTutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateConnection() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteCoachConnection(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteTutorantConnection(int tutorantID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAllCoachConnections(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetTutorantConnectionByID(int tutorantID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> UpdateConnectionByID(int tutorantID) {
            throw new NotImplementedException();
        }
    }
}
