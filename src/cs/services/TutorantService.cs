using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class TutorantService : ITutorantService {

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public TutorantService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }
        public Task<HttpResponseMessage> CreateTutorant() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteTutorantByID(int tutorantID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAllTutorants() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetTutorantByID(int tutorantID) {
            throw new NotImplementedException();
        }
    }
}
