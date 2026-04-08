using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Hubs;
using SimTrainer.Api.Infrastructure;
using SimTrainer.Api.Services;
using SimTrainer.Api.Simulation;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<SimTrainerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddSingleton<CprSimulator>();
builder.Services.AddSingleton<VitalSignsGenerator>();
builder.Services.AddSingleton<DeviceSimulatorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DeviceSimulatorService>());
builder.Services.AddScoped<ScenarioService>(sp =>
    new ScenarioService(
        sp.GetRequiredService<VitalSignsGenerator>(),
        sp.GetRequiredService<SessionService>()));

// SignalR + Controllers
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SimTrainerDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (var retries = 0; retries < 30; retries++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migrated successfully");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Database not ready (attempt {Attempt}): {Message}", retries + 1, ex.Message);
            Thread.Sleep(2000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.MapHub<CprHub>("/hubs/cpr");
app.MapHub<MonitorHub>("/hubs/monitor");

app.Run();

public partial class Program { }
