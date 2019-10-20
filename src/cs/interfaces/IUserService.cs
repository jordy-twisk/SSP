using System.Threading.Tasks;
using System.Net.Http;

namespace TinderCloneV1 {
    public interface IUserService {
        Task<HttpResponseMessage> GetAll();
        Task<HttpResponseMessage> GetStudentByID(int userID);
        Task<HttpResponseMessage> CreateStudentByID(int userID);
    } 
}
