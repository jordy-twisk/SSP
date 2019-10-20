using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface IUserService {
        Task<HttpResponseMessage> GetAll();
        Task<HttpResponseMessage> GetStudent(int ID);
        Task<HttpResponseMessage> PutStudent(int ID);
    } 
}
