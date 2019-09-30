using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net;
using Microsoft.Azure.WebJobs.Host;

namespace TinderCloneV1{
    public static class TestConnection{
        [FunctionName("TestConnection")]
        [Obsolete]
        public static async Task<HttpResponseMessage>Run([HttpTrigger(AuthorizationLevel.Function,"get", "post", Route = null)]HttpRequestMessage req, TraceWriter log) {
            log.Info("C# HTTP trigger function processed a request.");

            try{
                string str = Environment.GetEnvironmentVariable("sqldb_connection");

                using (SqlConnection connection = new SqlConnection(str)){
                    await connection.OpenAsync();
                    return req.CreateResponse(HttpStatusCode.OK, $"The database connection is: {connection.State}");
                }
            }
            catch (SqlException sqlex){
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following SqlException happened: {sqlex.Message}");
            }
            catch (Exception ex){
                return req.CreateResponse(HttpStatusCode.BadRequest, $"The following Exception happened: {ex.Message}");
            }
        }
    }
}
