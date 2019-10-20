using System;
using System.Data.SqlClient;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(TinderCloneV1.Startup))]
namespace TinderCloneV1 {
    class Startup : FunctionsStartup {
        public override void Configure(IFunctionsHostBuilder builder) {

            string sqlconnection = Environment.GetEnvironmentVariable("sqldb_connection");

            //builder.Services.AddDbContext<StudentController>(options => options.UseSqlServer(sqlconnection));

            //builder.Services.AddScoped<SqlConnection, StudentController>();

            // ExceptionHandler exceptionHandler = new ExceptionHandler(0);

            using (SqlConnection connection = new SqlConnection(sqlconnection)) {
                try {
                    connection.Open();

                    builder.Services.AddTransient<IUserService, UserService>();
                    builder.Services.AddLogging();


                } catch (SqlException e) {

                    // exceptionHandler.ServiceUnavailable();
                }
            }
        }
    }
}
