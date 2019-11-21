using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1 {
    public interface ICoachService {
        Task<HttpResponseMessage> GetAllCoachProfiles();
        Task<HttpResponseMessage> CreateCoachProfile(JObject requestBodyData);
        Task<HttpResponseMessage> GetCoachProfileByID(int coachID);
        Task<HttpResponseMessage> DeleteCoachProfileByID(int coachID);
        Task<HttpResponseMessage> GetCoachByID(int coachID);
        Task<HttpResponseMessage> UpdateCoachByID(int coachID);
    }
}
