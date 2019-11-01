using System.Threading.Tasks;
using System.Net.Http;
using System.Data.SqlClient;

namespace TinderCloneV1 {
    public interface IStudentService {
        Task<HttpResponseMessage> GetAllStudents();
        Task<HttpResponseMessage> GetStudentByID(int studentID);
        Task<HttpResponseMessage> UpdateStudentByID(int studentID);
    } 
}
