using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ICoachTutorantService {
        Task<HttpResponseMessage> GetAllCoachConnections(int coachID);
        Task<HttpResponseMessage> CreateConnection();
        Task<HttpResponseMessage> DeleteCoachConnection(int coachID);
        Task<HttpResponseMessage> DeleteTutorantConnection(int tutorantID);
        Task<HttpResponseMessage> GetTutorantConnectionByID(int tutorantID);
        Task<HttpResponseMessage> UpdateConnectionByID(int tutorantID);
    }
}
