using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ITutorantService {
        Task<HttpResponseMessage> GetAllTutorantProfiles();
        Task<HttpResponseMessage> CreateTutorantProfile();
        Task<HttpResponseMessage> GetTutorantProfileByID(int tutorantID);
        Task<HttpResponseMessage> DeleteTutorantProfileByID(int tutorantID);
    }
}
