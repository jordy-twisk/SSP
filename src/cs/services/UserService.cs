using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TinderCloneV1
{
    class UserService{
        [Obsolete]
        public async Task<HttpResponseMessage> GetAll(HttpRequestMessage req, HttpRequest request, TraceWriter log){

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }
    }
}
