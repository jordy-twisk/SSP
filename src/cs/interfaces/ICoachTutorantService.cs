using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    interface ICoachTutorantService {
        Task<HttpResponseMessage> GetAllCoachConnections();
        Task<HttpResponseMessage> CreateConnection();
        Task<HttpResponseMessage> DeleteCoachConnection(int ID);
        Task<HttpResponseMessage> DeleteTutorantConnection(int ID);
        Task<HttpResponseMessage> GetTutorantConnectionByID(int ID);
        Task<HttpResponseMessage> UpdateConnection(int ID);
    }
}
