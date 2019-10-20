using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface IUserService {
        Task<HttpResponseMessage> GetAll();
        Task<HttpResponseMessage> GetStudentByID(int ID);
        Task<HttpResponseMessage> CreateStudentByID(int ID);
    } 
}
