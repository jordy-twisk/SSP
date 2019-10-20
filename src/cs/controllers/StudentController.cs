using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TinderCloneV1 {
    public class StudentController{

        IUserService userService;
        public StudentController(IUserService userService) {
            this.userService = userService;
        }

        [FunctionName("GetUsers")]
        public async Task<HttpResponseMessage> GetUsers([HttpTrigger(AuthorizationLevel.Function,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, ILogger log){

            userService = new UserService(req, request, log);

            return await userService.GetAll();
        }

        [FunctionName("GetUser")]
        public async Task<HttpResponseMessage> GetUser([HttpTrigger(AuthorizationLevel.Function, 
        "get", "put", Route = "student/{ID}")] HttpRequestMessage req, HttpRequest request, ILogger log, int ID) {

            userService = new UserService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await userService.GetStudent(ID);
            }
            
            else if (req.Method == HttpMethod.Put) {
                return await userService.PutStudent(ID);
            } 
            
            else {
                return new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent($"Student {ID} not found in the database")
                };
            }
        }
    }
}
