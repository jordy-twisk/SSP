using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {

    /* This is custom exceptionHandler which contains:

    - 400 BadRequest
    - 401 Unauthorized (not implemented)
    - 404 NotFound
    - 503 Service Not Available
    
    Throwing one of the exceptions also stores the message in the log*/
    public class ExceptionHandler {

        private readonly int ID;
        private ILogger log;
        public ExceptionHandler(ILogger log){
            this.log = log;
        }
        
        /*Undocumented filter parameter*/
        private readonly string badRequestMessage = $"{HttpStatusCode.BadRequest}: Bad request on the database";

        /*Student password is not accepted*/
        private readonly string notAuthorizedMessage = $"{HttpStatusCode.Unauthorized}: Student not authorized";

        /*Student number not in database*/
        private readonly string notFoundMessage = $"{HttpStatusCode.NotFound}: Requested data could not be found in the database";

        /*Outside service is unavailable*/
        private readonly string ServiceUnavailableMessage = $"{HttpStatusCode.ServiceUnavailable}: External component or service is unavailable at the moment. Our admin is notified by your call, thank you!";

        public HttpResponseMessage BadRequest(ILogger log) {
            log.LogError(badRequestMessage);

            return new HttpResponseMessage(HttpStatusCode.BadRequest) {
                Content = new StringContent(badRequestMessage)
            };
        }

        public HttpResponseMessage NotAuthorized(ILogger log) {
            log.LogError(notAuthorizedMessage);

            return new HttpResponseMessage(HttpStatusCode.Unauthorized) {
                Content = new StringContent(notAuthorizedMessage)
            };
        }

        public HttpResponseMessage NotFound() {
            log.LogError(notFoundMessage);

            return new HttpResponseMessage(HttpStatusCode.NotFound){
                Content = new StringContent(notFoundMessage)
            };
        }
       
        public HttpResponseMessage ServiceUnavailable(ILogger log){
            log.LogError(ServiceUnavailableMessage);

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){
                Content = new StringContent(ServiceUnavailableMessage)
            };
        }
    }
}
