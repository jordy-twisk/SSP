using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace TinderCloneV1
{
    public static class ApiInfo
    {
        [FunctionName("Status")]
        [Obsolete]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "status")]HttpRequest req, TraceWriter log)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("API is online")
            };
        }
    }
}
