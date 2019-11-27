using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TinderCloneV1 {
    class CoachDB {
        public CoachDB() {

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
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"{property.Name}, ";
                    }
                }
            }

            queryString_Student = databaseFunctions.RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ") ";

            /* Dynamically create the VALUES line of the SQL statement: */
            queryString_Student += "VALUES (";
            foreach (JProperty property in studentProfile.Properties()) {
                foreach (PropertyInfo props in newStudent.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        queryString_Student += $"@{property.Name}, ";
                    }
                }
            }

            queryString_Student = databaseFunctions.RemoveLastCharacters(queryString_Student, 2);
            queryString_Student += ");";

            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    /*The connection is automatically closed when going out of scope of the using block.
                      The connection may fail to open, in which case return a [503 Service Unavailable]. */
                    int studentCreated = 0;

                    connection.Open();

                    try {
                        /*Insert profile into the Student table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Student, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place 
                            To ensure generic code, a dynamic object is made to make a new Entity and be passed into the injection function */
                            dynamic dObject = newStudent;
                            databaseFunctions.AddSqlInjection(studentProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString_Student}");

                            // PREVIOUSLY: await command.ExecuteReaderAsync();
                            studentCreated = command.ExecuteNonQuery();
                        }

                        /*Insert profile into the Coach table.
                          The Query may fail, in which case a [400 Bad Request] is returned. */
                        using (SqlCommand command = new SqlCommand(queryString_Coach, connection)) {
                            /* Parameters are used to ensure no SQL injection can take place
                               To ensure generic code, a dynamic object is made to make a new Entity and be passed into the injection function*/
                            dynamic dObject = newCoach;
                            databaseFunctions.AddSqlInjection(coachProfile, dObject, command);

                            log.LogInformation($"Executing the following query: {queryString_Coach}");

                            /* Is the student query affected 0 rows (i.e.: Student did not create then
                               the coach cannot exists as well, so dont make the coach*/
                            if (studentCreated == 1) {
                                // PREVIOUSLY: await command.ExecuteReaderAsync();
                                command.ExecuteNonQuery();
                            }
                            else {
                                log.LogError($"Cannot create coach profile, student does not exists");
                                return exceptionHandler.BadRequest(log);
                            }
                        }
                    } catch (SqlException e) {
                        /* The Query may fail, in which case a [400 Bad Request] is returned.
                           Reasons for this failure may include a PK violation (entering an already existing studentID). */
                        log.LogError("SQL Query has failed to execute.");
                        log.LogError(e.Message);
                        return exceptionHandler.BadRequest(log);
                    }
                }
            } catch (SqlException e) {
                /* The connection may fail to open, in which case a [503 Service Unavailable] is returned. */
                log.LogError("SQL connection has failed to open.");
                log.LogError(e.Message);
                return exceptionHandler.ServiceUnavailable(log);
            }

            log.LogInformation($"{HttpStatusCode.Created} | Profile created succesfully.");
        }
}
