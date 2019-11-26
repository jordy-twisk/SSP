using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TinderCloneV1 {
    public interface IStudentService {
        Task<HttpResponseMessage> GetAllStudents(List<string> parameters, List<string> propertyNames);
        Task<HttpResponseMessage> GetStudentByID(int studentID);
        Task<HttpResponseMessage> UpdateStudentByID(int studentID, JObject requestBodyData);
    } 
}
