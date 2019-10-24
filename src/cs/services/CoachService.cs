using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    class CoachService : ICoachService {
        private readonly string environmentString = Environment.GetEnvironmentVariable("sqldb_connection");

        private readonly HttpRequestMessage req;
        private readonly HttpRequest request;
        private readonly ILogger log;

        public CoachService(HttpRequestMessage req, HttpRequest request, ILogger log) {
            this.req = req;
            this.request = request;
            this.log = log;
        }

        public Task<HttpResponseMessage> CreateCoach() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteCoachByID(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAllCoaches() {
            throw new NotImplementedException();
        }
        
        public Task<HttpResponseMessage> GetCoachAndWorkloadByID(int coachID) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetCoachByID(int coachID) {
            ExceptionHandler exceptionHandler = new ExceptionHandler(coachID);
            Coach newCoach;

            string queryString = $"SELECT * FROM [dbo].[Coach] WHERE studentID = {coachID};";
            log.LogInformation($"Executing the following query: {queryString}");

            try {
                using (SqlConnection connection = new SqlConnection(environmentString)) {
                    try {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(queryString, connection)) {
                            using (SqlDataReader reader = command.ExecuteReader()) {
                                if (!reader.HasRows) {
                                    return exceptionHandler.NotFoundException(log);
                                } else {
                                    while (reader.Read()) {
                                        newCoach = new Coach {
                                            studentID = reader.GetInt32(0),
                                            workload = reader.GetInt32(1)
                                        };
                                    }
                                }
                            }
                        }
                    } catch (SqlException e) {
                        log.LogError(e.Message);
                        return exceptionHandler.ServiceUnavailable(log);
                    } finally {
                        connection.Close();
                    }
                }
            } catch (SqlException e) {
                log.LogError(e.Message);
                return exceptionHandler.BadRequest(log);
            }

            var jsonToReturn = JsonConvert.SerializeObject(newCoach);
            log.LogInformation($"{HttpStatusCode.OK} | Data shown succesfully");

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public Task<HttpResponseMessage> UpdateCoachAndWorkloadByID(int coachID) {
                throw new NotImplementedException();
            }
        }
    }
