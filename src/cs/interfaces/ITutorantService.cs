using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1 {
    public interface ITutorantService {
        Task<HttpResponseMessage> GetAllTutorantProfiles();
        Task<HttpResponseMessage> CreateTutorantProfile(JObject requestBodydata);
        Task<HttpResponseMessage> GetTutorantProfileByID(int tutorantID);
        Task<HttpResponseMessage> DeleteTutorantProfileByID(int tutorantID);
    }
}
