using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ICoachTutorantService {
        Task<HttpResponseMessage> UpdateConnection();
        Task<HttpResponseMessage> GetAllConnectionsByCoachID(int coachID);
        Task<HttpResponseMessage> DeleteConnectionByCoachID(int coachID);
        Task<HttpResponseMessage> GetConnectionByTutorantID(int tutorantID);
        Task<HttpResponseMessage> CreateConnectionByTutorantID(int tutorantID);
        Task<HttpResponseMessage> DeleteConnectionByTutorantID(int tutorantID);
    }
}
