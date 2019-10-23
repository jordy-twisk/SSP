using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class UserController{

        IUserService userService;
        public UserController(IUserService userService) {
            this.userService = userService;
        }

        [FunctionName("Users")]
        public async Task<HttpResponseMessage> GetUsers([HttpTrigger(AuthorizationLevel.Function,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            userService = new UserService(req, request, log);

            return await userService.GetAll();
        }

        [FunctionName("UserByID")]
        public async Task<HttpResponseMessage> GetUser([HttpTrigger(AuthorizationLevel.Function, 
        "get", "put", Route = "student/{ID}")] HttpRequestMessage req, HttpRequest request, ILogger log, int ID) {

            userService = new UserService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await userService.GetStudentByID(ID);
            }
            else if (req.Method == HttpMethod.Put) {
                return await userService.UpdateUserByID(ID);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
