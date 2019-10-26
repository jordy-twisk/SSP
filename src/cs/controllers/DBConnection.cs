using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using System.Data.SqlClient;

namespace TinderCloneV1 {
    public static class DBConnection{
        [FunctionName("Status")]
        [Obsolete]
        public static async Task<HttpResponseMessage> Run ([HttpTrigger(AuthorizationLevel.Anonymous,"get", Route = null)]HttpRequestMessage req, TraceWriter log) {
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
