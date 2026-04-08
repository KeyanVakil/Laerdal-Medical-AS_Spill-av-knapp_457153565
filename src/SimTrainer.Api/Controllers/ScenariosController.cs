using Microsoft.AspNetCore.Mvc;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Controllers;

[ApiController]
[Route("api/scenarios")]
public class ScenariosController : ControllerBase
{
    private readonly ScenarioService _scenarioService;

    public ScenariosController(ScenarioService scenarioService)
    {
        _scenarioService = scenarioService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var scenarios = _scenarioService.GetAllScenarios();
        return Ok(scenarios.Select(kvp => new
        {
            Name = kvp.Key,
            Vitals = new
            {
                HeartRate = $"{kvp.Value.HeartRateMin}-{kvp.Value.HeartRateMax}",
                SpO2 = $"{kvp.Value.SpO2Min}-{kvp.Value.SpO2Max}",
                BloodPressure = $"{kvp.Value.SystolicBp}/{kvp.Value.DiastolicBp}",
                RespiratoryRate = $"{kvp.Value.RespRateMin}-{kvp.Value.RespRateMax}"
            }
        }));
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateScenarioRequest request)
    {
        var (success, name, transitionMs) = await _scenarioService.ActivateAsync(request.SessionId, request.ScenarioName);

        if (!success)
            return BadRequest(new { error = $"Unknown scenario: {request.ScenarioName}" });

        return Ok(new
        {
            Activated = true,
            ScenarioName = name,
            TransitionDurationMs = transitionMs
        });
    }
}

public record ActivateScenarioRequest(Guid SessionId, string ScenarioName);
