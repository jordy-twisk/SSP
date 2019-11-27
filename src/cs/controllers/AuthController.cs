using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
 * Note:
 * Authentications purposes only
 */

namespace TinderCloneV1 {
    public class AuthController {

        IAuthService authService;

        public AuthController (IAuthService authService) {
            this.authService = authService;
        }

        /*
        Route to /api/profile/coach
        GET: Gets all the coach profiles (Student and Coach table data)
        POST: Creates a new Coach profile
        */
        [FunctionName("Login")]
        public async Task<HttpResponseMessage> AuthLogin([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "auth/login")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            authService = new AuthService(req, request, log);

            if (req.Method == HttpMethod.Post) {
                return await authService.Login();
            }
            else {
                throw new NotImplementedException();
            }
        }


        [FunctionName("Register")]
        public async Task<HttpResponseMessage> AuthRegister([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "auth/register")] HttpRequestMessage req, HttpRequest request, ILogger log)
        {

            authService = new AuthService(req, request, log);

            if (req.Method == HttpMethod.Post)
            {
                return await authService.CreateAuth();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [FunctionName("TestToken")]
        public async Task<HttpResponseMessage> TestToken([HttpTrigger(AuthorizationLevel.Anonymous,
            "post", Route = "auth/testToken")] HttpRequestMessage req, HttpRequest request, ILogger log)
        {

            authService = new AuthService(req, request, log);

            if (req.Method == HttpMethod.Post)
            {
                /* Read from the requestBody */
                JObject jObject = null;
                using (StringReader reader = new StringReader(await req.Content.ReadAsStringAsync()))
                {
                    jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }
                return await authService.TestToken(jObject);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
