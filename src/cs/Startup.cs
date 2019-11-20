using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(TinderCloneV1.Startup))]
namespace TinderCloneV1 {

    /* The startup function to prevent Dependency Injection */
    class Startup : FunctionsStartup {
        public override void Configure(IFunctionsHostBuilder builder) {

            builder.Services.AddTransient<ICoachService, CoachService>();
            builder.Services.AddTransient<ICoachTutorantService, CoachTutorantService>();
            builder.Services.AddTransient<IMessageService, MessageService>();
            builder.Services.AddTransient<ITutorantService, TutorantService>();
            builder.Services.AddTransient<IStudentService, StudentService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddLogging();
        }
    }
}
    
