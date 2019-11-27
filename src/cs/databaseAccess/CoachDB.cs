using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class CoachDB {
        private readonly string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");
        private readonly ILogger log;

        public CoachDB(ILogger log) {
            this.log = log;
        }

        public Task<bool> CreateProfile(Coach newCoach, Student newStudent) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(log);

            /* All fields for the Coach table are required */
            string queryString_Coach = $@"INSERT INTO [dbo].[Coach] (studentID, workload)
                                            VALUES (@studentID, @workload);";

            /* The SQL query for the Students table has to be dynamically generated, as it contains many optional fields.
               By manually adding the columns to the query string (if they're present in the request body) we prevent
               SQL injection and ensure no illegitimate columnnames are entered into the SQL query. */

            /* Dynamically create the INSERT INTO line of the SQL statement: */
            string queryString_Student = $@"INSERT INTO [dbo].[Student] (";

            return Task.FromResult(true);
        }
    }
}
