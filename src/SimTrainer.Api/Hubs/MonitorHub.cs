using Microsoft.AspNetCore.SignalR;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Hubs;

public class MonitorHub : Hub
{
    private readonly DeviceSimulatorService _simulator;
    private readonly ILogger<MonitorHub> _logger;

    public MonitorHub(DeviceSimulatorService simulator, ILogger<MonitorHub> logger)
    {
        _simulator = simulator;
        _logger = logger;
    }

    public Task StartMonitoring(Guid sessionId)
    {
        _logger.LogInformation("Monitor session started: {SessionId}", sessionId);
        _simulator.StartMonitorSession(sessionId);
        return Task.CompletedTask;
    }

    public Task StopMonitoring(Guid sessionId)
    {
        _logger.LogInformation("Monitor session stopped: {SessionId}", sessionId);
        _simulator.StopMonitorSession();
        return Task.CompletedTask;
    }
}
