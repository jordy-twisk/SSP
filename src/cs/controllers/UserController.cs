using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1
{
    public class UserController{
        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        ExceptionHandler exceptionHandler = new ExceptionHandler(0);
        Task<HttpResponseMessage> httpResponseMessage = null;

        [FunctionName("GetUsers")]
        public async Task<HttpResponseMessage> GetUsers([HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route = "students/search")] HttpRequestMessage req, HttpRequest request, ILogger log){
            
            UserService userService = new UserService(req, request, log);

            using (SqlConnection connection = new SqlConnection(str)){
                try {
                    connection.Open();
                }
                catch (SqlException e) {
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }

                httpResponseMessage = userService.GetAll(connection);
                connection.Close();

            }
            return await httpResponseMessage;
        }

        [FunctionName("GetUser")]
        public async Task<HttpResponseMessage> GetUser([HttpTrigger(AuthorizationLevel.Anonymous, 
        "get", "post", "delete", "put", Route = "student/{ID}")] HttpRequestMessage req, HttpRequest request, int ID, ILogger log){

            UserService userService = new UserService(req, request, log);

            using (SqlConnection connection = new SqlConnection(str)) {
                try{
                    connection.Open();
                }
                catch (SqlException e){
                    log.LogInformation(e.Message);
                    return exceptionHandler.ServiceUnavailable(log);
                }
                
                if (req.Method == HttpMethod.Get){
                    httpResponseMessage = userService.GetStudent(ID, connection);
                } 
                else if (req.Method == HttpMethod.Put) {
                    httpResponseMessage = userService.PutStudent(ID, connection);
                }

                connection.Close();
            } 
            return await httpResponseMessage;
        }
    }
}
