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

        string notFoundMessage = $"{HttpStatusCode.NotFound}: Requested data could not be found in the database";

        [Obsolete]
        public HttpResponseMessage NotFoundException(TraceWriter log, int ID) {

            notFoundMessage += $", ID: {ID}";
            log.Info(notFoundMessage);

            return new HttpResponseMessage(HttpStatusCode.NotFound){
                Content = new StringContent(notFoundMessage)
            };
        }

    }
}
