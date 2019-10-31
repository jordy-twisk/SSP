﻿using System.Threading.Tasks;
using System.Net.Http;
using System.Data.SqlClient;

namespace TinderCloneV1 {
    public interface IUserService {
        Task<HttpResponseMessage> GetAllStudents();
        Task<HttpResponseMessage> GetStudentByID(int studentID);
        Task<HttpResponseMessage> UpdateStudentByID(int studentID);

        // string SafeGetString(this SqlDataReader reader, int index);

        // string SafeGetInt(this SqlDataReader reader, int index);
    } 
}
