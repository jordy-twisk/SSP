using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    interface ICoachService {
        Task<HttpResponseMessage> GetAllCoaches();
        Task<HttpResponseMessage> CreateCoach();
        Task<HttpResponseMessage> GetCoachByID(int ID);
        Task<HttpResponseMessage> DeleteCoachByID(int ID);
        Task<HttpResponseMessage> GetCoachAndWorkloadByID(int ID);
        Task<HttpResponseMessage> UpdateCoachAndWorkloadByID(int ID);

    }
}
