using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface IMessageService {
        Task<HttpResponseMessage> GetAllMessages(int coachID, int tutorantID);
        Task<HttpResponseMessage> GetMessageByID(int messageID);
        Task<HttpResponseMessage> UpdateMessageByID(int messageID);
        Task<HttpResponseMessage> DeleteMessageByID(int messageID);
        Task<HttpResponseMessage> CreateMessage();
    }
}
