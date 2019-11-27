using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1 {
    public interface ICoachTutorantService {
        Task<HttpResponseMessage> UpdateConnection(JObject ctObject);
        Task<HttpResponseMessage> GetAllConnectionsByCoachID(int coachID);
        Task<HttpResponseMessage> DeleteConnectionByCoachID(int coachID);
        Task<HttpResponseMessage> GetConnectionByTutorantID(int tutorantID);
        Task<HttpResponseMessage> CreateConnectionByTutorantID(int tutorantID, JObject tTocConnection);
        Task<HttpResponseMessage> DeleteConnectionByTutorantID(int tutorantID);
    }
}
