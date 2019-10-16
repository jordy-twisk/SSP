using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1
{
    public class ExceptionHandler{

        private int ID;
        public ExceptionHandler(int PK){
            ID = PK;
        }
        
        /*Undocumented filter parameter*/
        string badRequestMessage = $"{HttpStatusCode.BadRequest}: Bad request on the database";

        /*Student password is not accepted*/
        string notAuthorized = $"{HttpStatusCode.Unauthorized}: Student not authorized";

        /*Student number not in database*/
        string notFoundMessage = $"{HttpStatusCode.NotFound}: Requested data could not be found in the database";

        /*Too many requests*/
        string tooManyRequestsMessage = $"{HttpStatusCode.TooManyRequests}: Too many requests on the database";
        
        [Obsolete]
        public HttpResponseMessage NotFoundException(TraceWriter log) {
            notFoundMessage += $", ID: {ID}";
            log.Info(notFoundMessage);

            return new HttpResponseMessage(HttpStatusCode.NotFound){
                Content = new StringContent(notFoundMessage)
            };
        }

        [Obsolete]
        public HttpResponseMessage NotAuthorized(TraceWriter log){
            notAuthorized += $", ID: {ID}";
            log.Info(notAuthorized);

            return new HttpResponseMessage(HttpStatusCode.Unauthorized){
                Content = new StringContent(notAuthorized)
            };
        }
        [Obsolete]
        public HttpResponseMessage TooManyRequests(TraceWriter log){
            tooManyRequestsMessage += $", ID: {ID}";
            log.Info(tooManyRequestsMessage);

            return new HttpResponseMessage(HttpStatusCode.TooManyRequests){
                Content = new StringContent(tooManyRequestsMessage)
            };
        }
        [Obsolete]
        public HttpResponseMessage BadRequest(TraceWriter log){
            badRequestMessage += $", ID: {ID}";
            log.Info(badRequestMessage);

            return new HttpResponseMessage(HttpStatusCode.TooManyRequests){
                Content = new StringContent(badRequestMessage)
            };
        }
    }
}
