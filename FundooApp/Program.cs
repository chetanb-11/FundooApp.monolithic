using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NLog.Web;
using UserService.Application.Models;
using UserServices = UserService.Api.Extensions.ServiceExtensions;
using NoteServices = NoteService.Api.Extensions.ServiceExtensions;
using Swagger = UserService.Api.Extensions.SwaggerExtensions;

namespace FundooApp;

public class Startup
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var config = builder.Configuration;
        UserServices.AddJwtAuthentication(services, config);
        UserServices.AddDatabaseContext(services, config);
        NoteServices.AddDatabaseContext(services, config.GetSection("NoteDatabase"));
        UserServices.AddApplicationServices(services);
        NoteServices.AddApplicationServices(services);
        services.AddSingleton(TimeProvider.System);
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("Redis");
            options.InstanceName = "fundooapp:";
        });
        services.AddExceptionHandler<UserService.Api.Middlewares.GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddControllers(options => options.Conventions.Add(new TimingFilters()));
        services.AddEndpointsApiExplorer();
        Swagger.AddSwaggerDocumentation(services);
        
        services.AddCors(options =>
        {
            options.AddPolicy("frontend", policy => policy
                .WithOrigins("http://localhost:5173")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowAnyHeader()
            );
        });
        
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        var app = builder.Build();
        app.UseExceptionHandler();
        if (app.Environment.IsDevelopment())
            Swagger.UseSwaggerDocumentation(app);
        app.UseHttpsRedirection();
        app.UseCors("frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/", () => "Fundoo API Gateway");
        app.MapHealthChecks("/health");
        app.MapControllers();
        app.Run();
    }

    private sealed class TimingFilters : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var filter = controller.ControllerType == typeof(NoteService.Api.Controllers.NotesController)
                ? typeof(NoteService.Api.Filters.LogExecutionTimeFilter)
                : typeof(UserService.Api.Filters.LogExecutionTimeFilter);
            controller.Filters.Add(new Microsoft.AspNetCore.Mvc.TypeFilterAttribute(filter));
        }
    }
}
