using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(TinderCloneV1.MainController))]
namespace TinderCloneV1 {

    internal static class ApplicationLogging {
        internal static ILoggerFactory LoggerFactory { get; set; }
        internal static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        internal static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }

    class MainController : FunctionsStartup {

        string str = Environment.GetEnvironmentVariable("sqldb_connection");
        public override void Configure(IFunctionsHostBuilder builder) {

            builder.Services.AddTransient<IUserService, UserService>();
            //builder.Services.AddLogging();

            ExceptionHandler exceptionHandler = new ExceptionHandler(0);
            ILogger log = ApplicationLogging.CreateLogger<MainController>();

            using (SqlConnection connection = new SqlConnection(str)) {
                try{
                    connection.Open();

                    StudentController studentController = new StudentController(connection);

                } catch (SqlException e) {
                    log.LogError(e.Message);
                    exceptionHandler.ServiceUnavailable(log);
                }
            }
        }
    }
}
