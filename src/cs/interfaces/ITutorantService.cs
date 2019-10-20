using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    interface ITutorantService {
        Task<HttpResponseMessage> GetAllTutorants();
        Task<HttpResponseMessage> CreateTutorant();
        Task<HttpResponseMessage> GetTutorantByID(int ID);
        Task<HttpResponseMessage> DeleteTutorantByID(int ID);
    }
}
