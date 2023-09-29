using Microsoft.AspNetCore.Authentication.Negotiate;
using WebapiPlayground.Hubs;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

/* 
 * ASPNETCORE_ENVIRONMENT
 * 
 * 
 */


var appsettings = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(appsettings)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting Web Host");
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddSingleton<INotificationService, NotificationService>();

    // Add services to the container.
    var corsOriginsAllowed = builder.Configuration.GetSection("CorsOrigin").Get<string[]>();
    // var corsOriginsAllowed2 = builder.Configuration.GetSection("CorsOrigin").GetChildren().Select(c => c.Value).ToArray();
    builder.Services.AddControllers();
    builder.Services.AddHealthChecks()
        .AddCheck("gsl1", () => HealthCheckResult.Healthy("SQL 1 Connection is good"))
        .AddUrlGroup(new Uri("https://abc7fghafgar.com"), "gesService url check")
        .AddFolder(x => x.AddFolder("../logs"), "LogFolder")
        .AddCheck("gsl2", () => HealthCheckResult.Unhealthy("SQL 2 Connection is bad", new Exception("DB does not exist")));



    builder.Services.AddSignalR();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
       .AddNegotiate();

    //builder.Services.AddAuthorization(options =>
    //{
    //    // By default, all incoming requests will be authorized according to the default policy.
    //    options.FallbackPolicy = options.DefaultPolicy;
    //});
    builder.Services.AddAuthorization();

    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy(name: "corspolicy", builder =>
         {
             builder
             .WithOrigins(corsOriginsAllowed)
             .AllowAnyHeader()
             .AllowAnyMethod()

             //Can't have AllowAnyOrigin() with AllowCredentials.  Will throw error
             //.AllowAnyOrigin();
             .AllowCredentials();
         });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    // The request handling pipeline is composed as a series of middleware components
    // Used to specify how apps respond to HTTP requests
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("corspolicy");
    //app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    app.MapHub<RTHub>("/rt");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


