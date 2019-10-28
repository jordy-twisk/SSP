using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ICoachTutorantService {
        Task<HttpResponseMessage> CreateConnection();
        Task<HttpResponseMessage> GetAllCoachConnections(int coachID);
        Task<HttpResponseMessage> DeleteCoachConnection(int coachID);
        Task<HttpResponseMessage> GetTutorantConnectionByID(int tutorantID);
        Task<HttpResponseMessage> UpdateConnectionByID(int tutorantID);
        Task<HttpResponseMessage> DeleteTutorantConnection(int tutorantID);
    }
}
