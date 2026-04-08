using Microsoft.AspNetCore.Mvc;
using SimTrainer.Api.Domain.Enums;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly SessionService _sessionService;

    public SessionsController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        [FromQuery] Guid? learnerId,
        [FromQuery] SessionType? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var sessions = await _sessionService.GetSessionsAsync(learnerId, type, from, to);
        return Ok(sessions.Select(s => new
        {
            s.Id,
            s.LearnerId,
            LearnerName = s.Learner?.Name,
            s.SessionType,
            s.StartedAt,
            s.EndedAt,
            s.OverallScore
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var session = await _sessionService.GetSessionAsync(id);
        if (session == null) return NotFound();

        return Ok(new
        {
            session.Id,
            session.LearnerId,
            LearnerName = session.Learner?.Name,
            session.SessionType,
            session.StartedAt,
            session.EndedAt,
            session.OverallScore,
            Compressions = session.Compressions.Select(c => new
            {
                c.Id, c.Timestamp, c.DepthCm, c.RateBpm, c.FullRecoil, c.QualityScore
            }),
            Vitals = session.Vitals.Select(v => new
            {
                v.Id, v.Timestamp, v.HeartRate, v.SpO2, v.SystolicBp, v.DiastolicBp, v.RespiratoryRate
            }),
            ScenarioChanges = session.ScenarioChanges.Select(sc => new
            {
                sc.Id, sc.Timestamp, sc.ScenarioName, sc.PreviousScenario
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var session = await _sessionService.StartSessionAsync(request.LearnerId, request.SessionType);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, new
        {
            session.Id,
            session.LearnerId,
            session.SessionType,
            session.StartedAt
        });
    }

    [HttpPut("{id:guid}/end")]
    public async Task<IActionResult> EndSession(Guid id)
    {
        var session = await _sessionService.EndSessionAsync(id);
        if (session == null) return NotFound();

        return Ok(new
        {
            session.Id,
            session.LearnerId,
            session.SessionType,
            session.StartedAt,
            session.EndedAt,
            session.OverallScore
        });
    }

    [HttpGet("{id:guid}/compressions")]
    public async Task<IActionResult> GetCompressions(Guid id)
    {
        var compressions = await _sessionService.GetCompressionsAsync(id);
        return Ok(compressions.Select(c => new
        {
            c.Id, c.Timestamp, c.DepthCm, c.RateBpm, c.FullRecoil, c.QualityScore
        }));
    }

    [HttpGet("{id:guid}/vitals")]
    public async Task<IActionResult> GetVitals(Guid id)
    {
        var vitals = await _sessionService.GetVitalsAsync(id);
        return Ok(vitals.Select(v => new
        {
            v.Id, v.Timestamp, v.HeartRate, v.SpO2, v.SystolicBp, v.DiastolicBp, v.RespiratoryRate
        }));
    }

    [HttpGet("{id:guid}/scenarios")]
    public async Task<IActionResult> GetScenarios(Guid id)
    {
        var changes = await _sessionService.GetScenarioChangesAsync(id);
        return Ok(changes.Select(sc => new
        {
            sc.Id, sc.Timestamp, sc.ScenarioName, sc.PreviousScenario
        }));
    }
}

public record StartSessionRequest(Guid LearnerId, SessionType SessionType);
