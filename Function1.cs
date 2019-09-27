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

namespace TinderCloneV1
{
    public static class Function1
    {
       [FunctionName("DatabaseCleanup")]
        public static async Task Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, ILogger log) {
            // Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("Server=tcp:tinderclone.database.windows.net,1433;Initial Catalog=TinderCloneDB;" +
                                                         "Persist Security Info=False;User ID={moschbarend};Password={Bel32mac};" +
                                                         "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection" +
                                                         "Timeout=30;");

            using (SqlConnection conn = new SqlConnection(str)) {
                conn.Open();
                /*  var text = "UPDATE SalesLT.SalesOrderHeader SET [Status] = 5  WHERE ShipDate < GetDate();";

                // Try to add a CREATE TABLE Statement to try to connection to the database
                var text = "UPDATE SalesLT.SalesOrderHeader SET [Status] = 5  WHERE ShipDate < GetDate();";

                using (SqlCommand cmd = new SqlCommand(text, conn)){
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were created");
                }
                */
            }
        }
    }
}
