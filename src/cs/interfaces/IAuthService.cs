using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface IAuthService {
        Task<HttpResponseMessage> CreateAuth();
        Task<HttpResponseMessage> Login();
        Task<HttpResponseMessage> TestToken();
    }
}
