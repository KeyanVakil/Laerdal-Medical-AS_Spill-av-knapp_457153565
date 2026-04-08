using Microsoft.AspNetCore.SignalR;
using SimTrainer.Api.Domain.Scoring;
using SimTrainer.Api.Hubs;
using SimTrainer.Api.Simulation;

namespace SimTrainer.Api.Services;

public class DeviceSimulatorService : BackgroundService
{
    private readonly IHubContext<CprHub> _cprHub;
    private readonly IHubContext<MonitorHub> _monitorHub;
    private readonly CprSimulator _cprSimulator;
    private readonly VitalSignsGenerator _vitalSignsGenerator;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeviceSimulatorService> _logger;

    // Active session tracking
    private Guid? _activeCprSessionId;
    private Guid? _activeMonitorSessionId;
    private readonly object _lock = new();

    public DeviceSimulatorService(
        IHubContext<CprHub> cprHub,
        IHubContext<MonitorHub> monitorHub,
        CprSimulator cprSimulator,
        VitalSignsGenerator vitalSignsGenerator,
        IServiceScopeFactory scopeFactory,
        ILogger<DeviceSimulatorService> logger)
    {
        _cprHub = cprHub;
        _monitorHub = monitorHub;
        _cprSimulator = cprSimulator;
        _vitalSignsGenerator = vitalSignsGenerator;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void StartCprSession(Guid sessionId)
    {
        lock (_lock)
        {
            _activeCprSessionId = sessionId;
            _cprSimulator.Reset();
        }
    }

    public void StopCprSession()
    {
        lock (_lock) { _activeCprSessionId = null; }
    }

    public void StartMonitorSession(Guid sessionId)
    {
        lock (_lock) { _activeMonitorSessionId = sessionId; }
    }

    public void StopMonitorSession()
    {
        lock (_lock) { _activeMonitorSessionId = null; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device simulator service started");

        var cprTimer = Task.Run(() => RunCprLoop(stoppingToken), stoppingToken);
        var monitorTimer = Task.Run(() => RunMonitorLoop(stoppingToken), stoppingToken);

        await Task.WhenAll(cprTimer, monitorTimer);
    }

    private async Task RunCprLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Guid? sessionId;
            lock (_lock) { sessionId = _activeCprSessionId; }

            if (sessionId.HasValue)
            {
                try
                {
                    var (depth, rate, recoil) = _cprSimulator.GenerateCompression();
                    var score = CprQualityScorer.ScoreCompression(depth, rate, recoil);

                    // Persist to database
                    using var scope = _scopeFactory.CreateScope();
                    var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
                    var compression = await sessionService.AddCompressionAsync(sessionId.Value, depth, rate, recoil);

                    // Compute running overall score
                    var compressions = await sessionService.GetCompressionsAsync(sessionId.Value);
                    var overallScore = compressions.Count > 0
                        ? compressions.Average(c => c.QualityScore)
                        : 0m;

                    // Push to clients
                    await _cprHub.Clients.All.SendAsync("ReceiveCompression", new
                    {
                        compression.Id,
                        compression.SessionId,
                        compression.Timestamp,
                        compression.DepthCm,
                        compression.RateBpm,
                        compression.FullRecoil,
                        compression.QualityScore,
                        DepthFeedback = CprQualityScorer.GetDepthFeedback(depth),
                        RateFeedback = CprQualityScorer.GetRateFeedback(rate)
                    }, ct);

                    await _cprHub.Clients.All.SendAsync("SessionScore", Math.Round(overallScore, 1), ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CPR simulation loop");
                }
            }

            // ~2 compressions per second (simulating ~120bpm)
            await Task.Delay(500, ct);
        }
    }

    private async Task RunMonitorLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Guid? sessionId;
            lock (_lock) { sessionId = _activeMonitorSessionId; }

            if (sessionId.HasValue)
            {
                try
                {
                    var (hr, spo2, sys, dia, rr) = _vitalSignsGenerator.GenerateVitals();

                    // Persist to database
                    using var scope = _scopeFactory.CreateScope();
                    var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
                    await sessionService.AddVitalSnapshotAsync(sessionId.Value, hr, spo2, sys, dia, rr);

                    var vitals = new
                    {
                        SessionId = sessionId.Value,
                        Timestamp = DateTime.UtcNow,
                        HeartRate = hr,
                        SpO2 = spo2,
                        SystolicBp = sys,
                        DiastolicBp = dia,
                        RespiratoryRate = rr
                    };

                    await _monitorHub.Clients.All.SendAsync("ReceiveVitals", vitals, ct);

                    // Trigger alarms for cardiac arrest
                    if (_vitalSignsGenerator.CurrentScenario == "Cardiac Arrest")
                    {
                        await _monitorHub.Clients.All.SendAsync("AlarmTriggered", "cardiac_arrest", ct);
                    }
                    else if (spo2 < 90)
                    {
                        await _monitorHub.Clients.All.SendAsync("AlarmTriggered", "low_spo2", ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitor simulation loop");
                }
            }

            // 1Hz updates
            await Task.Delay(1000, ct);
        }
    }
}
