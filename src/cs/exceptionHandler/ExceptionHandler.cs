using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace TinderCloneV1 {
    public class ExceptionHandler {

        private readonly int ID;
        
        public ExceptionHandler(int ID){
            this.ID = ID;
        }
        
        /*Undocumented filter parameter*/
        private readonly string badRequestMessage = $"{HttpStatusCode.BadRequest}: Bad request on the database";

        /*Student password is not accepted*/
        private readonly string notAuthorizedMessage = $"{HttpStatusCode.Unauthorized}: Student not authorized";

        /*Student number not in database*/
        private readonly string notFoundMessage = $"{HttpStatusCode.NotFound}: Requested data could not be found in the database";

        /*Too many requests*/
        private readonly string tooManyRequestsMessage = $"{HttpStatusCode.TooManyRequests}: Too many requests on the database";

        /*Outside service is unavailable*/
        private readonly string ServiceUnavailableMessage = $"{HttpStatusCode.ServiceUnavailable}: External component or service is unavailable at the moment. Our admin is notified by your call, thank you!";

        public HttpResponseMessage NotFoundException(ILogger log) {
            LogMessage(ID, notFoundMessage, log);

            return new HttpResponseMessage(HttpStatusCode.NotFound){
                Content = new StringContent(notFoundMessage)
            };
        }

        public HttpResponseMessage NotAuthorized(ILogger log){
            LogMessage(ID, notAuthorizedMessage, log);

            return new HttpResponseMessage(HttpStatusCode.Unauthorized){
                Content = new StringContent(notAuthorizedMessage)
            };
        }
        public HttpResponseMessage TooManyRequests(ILogger log){
            LogMessage(ID, tooManyRequestsMessage, log);

            return new HttpResponseMessage(HttpStatusCode.TooManyRequests){
                Content = new StringContent(tooManyRequestsMessage)
            };
        }
        public HttpResponseMessage BadRequest(ILogger log){
            LogMessage(ID, badRequestMessage, log);

            return new HttpResponseMessage(HttpStatusCode.BadRequest){
                Content = new StringContent(badRequestMessage)
            };
        }

        public HttpResponseMessage ServiceUnavailable(ILogger log){
            log.LogInformation(ServiceUnavailableMessage);

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){
                Content = new StringContent(ServiceUnavailableMessage)
            };
        }

        public void LogMessage(int ID, string message, ILogger log) {
            if (ID != 0) {
                message += $", Primary Key: {ID}";
            }
            log.LogError(message);
        }
    }
}
