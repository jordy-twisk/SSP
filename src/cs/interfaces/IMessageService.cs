using System.Threading.Tasks;
using System.Net.Http;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1 {
    public interface IMessageService {
        Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID);
        Task<HttpResponseMessage> GetMessageByID(int messageID);
        Task<HttpResponseMessage> UpdateMessageByID(int messageID, JObject requestBodyData);
        Task<HttpResponseMessage> DeleteMessageByID(int messageID);
        Task<HttpResponseMessage> CreateMessage(JObject requestBodyData);
    }
}
