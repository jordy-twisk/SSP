using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class CoachService : ICoachService {

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateCoach() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteCoachByID(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAllCoaches() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetCoachAndWorkloadByID(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetCoachByID(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> UpdateCoachAndWorkloadByID(int coachID) {
            throw new NotImplementedException();
        }
    }
}
