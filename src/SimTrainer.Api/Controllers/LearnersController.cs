using Microsoft.AspNetCore.Mvc;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Controllers;

[ApiController]
[Route("api/learners")]
public class LearnersController : ControllerBase
{
    private readonly LearnerService _learnerService;

    public LearnersController(LearnerService learnerService)
    {
        _learnerService = learnerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var learners = await _learnerService.GetAllAsync();
        return Ok(learners.Select(l => new
        {
            l.Id,
            l.Name,
            l.Role,
            l.CreatedAt,
            SessionCount = l.Sessions.Count,
            AverageScore = l.Sessions
                .Where(s => s.OverallScore.HasValue)
                .Select(s => s.OverallScore!.Value)
                .DefaultIfEmpty(0)
                .Average()
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var learner = await _learnerService.GetByIdAsync(id);
        if (learner == null) return NotFound();

        return Ok(new
        {
            learner.Id,
            learner.Name,
            learner.Role,
            learner.CreatedAt,
            SessionCount = learner.Sessions.Count,
            AverageScore = learner.Sessions
                .Where(s => s.OverallScore.HasValue)
                .Select(s => s.OverallScore!.Value)
                .DefaultIfEmpty(0)
                .Average(),
            Sessions = learner.Sessions.Select(s => new
            {
                s.Id,
                s.SessionType,
                s.StartedAt,
                s.EndedAt,
                s.OverallScore
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLearnerRequest request)
    {
        var learner = await _learnerService.CreateAsync(request.Name, request.Role);
        return CreatedAtAction(nameof(GetById), new { id = learner.Id }, new
        {
            learner.Id,
            learner.Name,
            learner.Role,
            learner.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateLearnerRequest request)
    {
        var learner = await _learnerService.UpdateAsync(id, request.Name, request.Role);
        if (learner == null) return NotFound();

        return Ok(new { learner.Id, learner.Name, learner.Role, learner.CreatedAt });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _learnerService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public record CreateLearnerRequest(string Name, string Role);
