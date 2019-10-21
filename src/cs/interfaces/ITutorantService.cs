using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface ITutorantService {
        Task<HttpResponseMessage> GetAllTutorants();
        Task<HttpResponseMessage> CreateTutorant();
        Task<HttpResponseMessage> GetTutorantByID(int tutorantID);
        Task<HttpResponseMessage> DeleteTutorantByID(int tutorantID);
    }
}
