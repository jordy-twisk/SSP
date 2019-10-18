using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

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

        /*Outside service is unavailable*/
        string ServiceUnavailableMessage = $"{HttpStatusCode.ServiceUnavailable}: External component or service is unavailable at the moment. Our admin is notified by your call, thank you!";

        public HttpResponseMessage NotFoundException(ILogger log) {
            notFoundMessage += $", ID: {ID}";
            log.LogInformation(notFoundMessage);

            return new HttpResponseMessage(HttpStatusCode.NotFound){
                Content = new StringContent(notFoundMessage)
            };
        }

        public HttpResponseMessage NotAuthorized(ILogger log){
            notAuthorized += $", ID: {ID}";
            log.LogInformation(notAuthorized);

            return new HttpResponseMessage(HttpStatusCode.Unauthorized){
                Content = new StringContent(notAuthorized)
            };
        }
        public HttpResponseMessage TooManyRequests(ILogger log){
            tooManyRequestsMessage += $", ID: {ID}";
            log.LogInformation(tooManyRequestsMessage);

            return new HttpResponseMessage(HttpStatusCode.TooManyRequests){
                Content = new StringContent(tooManyRequestsMessage)
            };
        }
        public HttpResponseMessage BadRequest(ILogger log){
            badRequestMessage += $", ID: {ID}";
            log.LogInformation(badRequestMessage);

            return new HttpResponseMessage(HttpStatusCode.TooManyRequests){
                Content = new StringContent(badRequestMessage)
            };
        }

        public HttpResponseMessage ServiceUnavailable(ILogger log)
        {
            log.LogInformation(ServiceUnavailableMessage);

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){
                Content = new StringContent(ServiceUnavailableMessage)
            };
        }
    }
}
