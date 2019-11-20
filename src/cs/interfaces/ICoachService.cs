using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ICoachService {
        string GetAllCoachProfiles();
        Task<HttpResponseMessage> CreateCoachProfile();
        Task<HttpResponseMessage> GetCoachProfileByID(int coachID);
        Task<HttpResponseMessage> DeleteCoachProfileByID(int coachID);
        Task<HttpResponseMessage> GetCoachByID(int coachID);
        Task<HttpResponseMessage> UpdateCoachByID(int coachID);
    }
}
