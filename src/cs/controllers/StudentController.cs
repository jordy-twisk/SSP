using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class StudentController{

        IStudentService userService;
        public StudentController(IStudentService userService) {
            this.userService = userService;
        }

        /* 
        Route to /api/students/search
        GET: to get all Students with query parameters
        */
        [FunctionName("GetAllStudents")]
        public async Task<HttpResponseMessage> GetStudents([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "students/search")] HttpRequestMessage request, HttpRequest queryRequest, ILogger log) {

            userService = new StudentService(log);

            if (request.Method == HttpMethod.Get) {
                string isEmpty = null;
                PropertyInfo[] properties = typeof(Student).GetProperties();
                List<string> queryParameters = new List<string>();
                List<string> propertyNames = new List<string>();

                if (queryRequest.Query.Count != 0) {
                    foreach (PropertyInfo p in properties) {
                        if (queryRequest.Query[p.Name] != isEmpty) {
                            queryParameters.Add(queryRequest.Query[p.Name]);
                            propertyNames.Add(p.Name);
                        }
                    }
                }

                return await userService.GetAllStudents(queryParameters, propertyNames);
            } 
            else {
                throw new NotImplementedException();
            }
        }

        /*
        Route to /api/student/{ID}
        Where ID is a studentID path parameter
        GET: Get a specific student by his ID
        PUT: Update a specific student by his ID 
        */
        [FunctionName("StudentByID")]
        public async Task<HttpResponseMessage> StudentByID([HttpTrigger(AuthorizationLevel.Anonymous, 
        "get", "put", Route = "student/{ID}")] HttpRequestMessage request, ILogger log, int ID) {

            userService = new StudentService(log);

            if (request.Method == HttpMethod.Get) {
                return await userService.GetStudentByID(ID);
            }
            else if (request.Method == HttpMethod.Put) {
                JObject newStudentProfile = null; 
                using (StringReader reader = new StringReader(await request.Content.ReadAsStringAsync())) {
                    newStudentProfile = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                }
                return await userService.UpdateStudentByID(ID, newStudentProfile);
            } 
            else {
                throw new NotImplementedException();
            }
        }
    }
}
