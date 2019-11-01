using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class UserController{

        IStudentService userService;
        public UserController(IStudentService userService) {
            this.userService = userService;
        }

        /* 
        Route to /api/students/search
        GET: to get all Students with query parameters
        */
        [FunctionName("GetAllStudents")]
        public async Task<HttpResponseMessage> GetStudents([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, ILogger log) {

            userService = new StudentService(req, request, log);

            return await userService.GetAllStudents();
        }

        /*
        Route to /api/student/{ID}
        Where ID is a studentID path parameter
        GET: Get a specific student by his ID
        PUT: Update a specific student by his ID 
        */
        [FunctionName("StudentByID")]
        public async Task<HttpResponseMessage> StudentByID([HttpTrigger(AuthorizationLevel.Anonymous, 
        "get", "put", Route = "student/{ID}")] HttpRequestMessage req, HttpRequest request, ILogger log, int ID) {

            userService = new StudentService(req, request, log);

            if (req.Method == HttpMethod.Get) {
                return await userService.GetStudentByID(ID);
            }
            else if (req.Method == HttpMethod.Put) {
                return await userService.UpdateStudentByID(ID);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
