using Microsoft.AspNetCore.SignalR;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Hubs;

public class CprHub : Hub
{
    private readonly DeviceSimulatorService _simulator;
    private readonly ILogger<CprHub> _logger;

    public CprHub(DeviceSimulatorService simulator, ILogger<CprHub> logger)
    {
        _simulator = simulator;
        _logger = logger;
    }

    public Task StartSession(Guid sessionId)
    {
        _logger.LogInformation("CPR session started: {SessionId}", sessionId);
        _simulator.StartCprSession(sessionId);
        return Task.CompletedTask;
    }

    public Task StopSession(Guid sessionId)
    {
        _logger.LogInformation("CPR session stopped: {SessionId}", sessionId);
        _simulator.StopCprSession();
        return Task.CompletedTask;
    }
}
