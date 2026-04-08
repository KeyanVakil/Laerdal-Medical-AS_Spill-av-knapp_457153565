using SimTrainer.Api.Simulation;

namespace SimTrainer.Api.Services;

public class ScenarioService
{
    private readonly VitalSignsGenerator _generator;
    private readonly SessionService _sessionService;

    public ScenarioService(VitalSignsGenerator generator, SessionService sessionService)
    {
        _generator = generator;
        _sessionService = sessionService;
    }

    public IReadOnlyDictionary<string, VitalRanges> GetAllScenarios() => PatientScenario.Scenarios;

    public async Task<(bool Success, string ScenarioName, int TransitionMs)> ActivateAsync(Guid sessionId, string scenarioName)
    {
        if (!PatientScenario.Scenarios.ContainsKey(scenarioName))
            return (false, scenarioName, 0);

        var previousScenario = _generator.CurrentScenario;
        _generator.SetScenario(scenarioName);
        await _sessionService.AddScenarioChangeAsync(sessionId, scenarioName, previousScenario);

        return (true, scenarioName, 8000);
    }
}
