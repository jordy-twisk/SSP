using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    interface IMessageService {
        Task<HttpResponseMessage> GetAllMessages();
        Task<HttpResponseMessage> GetMessageByID();
        Task<HttpResponseMessage> UpdateMessageByID(int ID);
        Task<HttpResponseMessage> DeleteMessageByID(int ID);
        Task<HttpResponseMessage> CreateMessageByID(int ID);
    }
}
