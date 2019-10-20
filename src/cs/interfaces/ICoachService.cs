using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    interface ICoachService {
        Task<HttpResponseMessage> GetAllCoaches();
        Task<HttpResponseMessage> CreateCoach();
        Task<HttpResponseMessage> GetCoachByID(int coachID);
        Task<HttpResponseMessage> DeleteCoachByID(int coachID);
        Task<HttpResponseMessage> GetCoachAndWorkloadByID(int coachID);
        Task<HttpResponseMessage> UpdateCoachAndWorkloadByID(int coachID);

    }
}
