using Microsoft.AspNetCore.Mvc;
using SimTrainer.Api.Domain.Enums;
using SimTrainer.Api.Domain.Scoring;
using SimTrainer.Api.Services;

namespace SimTrainer.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly LearnerService _learnerService;

    public AnalyticsController(SessionService sessionService, LearnerService learnerService)
    {
        _sessionService = sessionService;
        _learnerService = learnerService;
    }

    [HttpGet("learner/{id:guid}")]
    public async Task<IActionResult> GetLearnerAnalytics(Guid id)
    {
        var learner = await _learnerService.GetByIdAsync(id);
        if (learner == null) return NotFound();

        var cprSessions = learner.Sessions
            .Where(s => s.SessionType == SessionType.CprTraining && s.OverallScore.HasValue)
            .OrderBy(s => s.StartedAt)
            .ToList();

        var scores = cprSessions.Select(s => s.OverallScore!.Value).ToList();
        var trend = AnalyticsEngine.DetermineTrend(scores);

        // Gather all compressions across sessions for aggregate suggestions
        var allCompressions = new List<Domain.Entities.CompressionEvent>();
        foreach (var session in cprSessions)
        {
            var compressions = await _sessionService.GetCompressionsAsync(session.Id);
            allCompressions.AddRange(compressions);
        }

        var suggestions = AnalyticsEngine.GenerateSuggestions(allCompressions);

        return Ok(new
        {
            LearnerId = id,
            TotalSessions = cprSessions.Count,
            AverageScore = scores.Count > 0 ? Math.Round(scores.Average(), 1) : 0m,
            BestScore = scores.Count > 0 ? scores.Max() : 0m,
            RecentTrend = trend,
            SessionScores = cprSessions.Select(s => new
            {
                s.Id,
                Date = s.StartedAt.ToString("yyyy-MM-dd"),
                Score = s.OverallScore!.Value
            }),
            Suggestions = suggestions
        });
    }

    [HttpGet("session/{id:guid}")]
    public async Task<IActionResult> GetSessionAnalytics(Guid id)
    {
        var session = await _sessionService.GetSessionAsync(id);
        if (session == null) return NotFound();

        var compressions = session.Compressions;
        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);

        var depthDistribution = new
        {
            Below5 = compressions.Count(c => c.DepthCm < 5.0m),
            Optimal = compressions.Count(c => c.DepthCm >= 5.0m && c.DepthCm <= 6.0m),
            Above6 = compressions.Count(c => c.DepthCm > 6.0m)
        };

        var rateDistribution = new
        {
            Below100 = compressions.Count(c => c.RateBpm < 100),
            Optimal = compressions.Count(c => c.RateBpm >= 100 && c.RateBpm <= 120),
            Above120 = compressions.Count(c => c.RateBpm > 120)
        };

        return Ok(new
        {
            SessionId = id,
            session.StartedAt,
            session.EndedAt,
            session.OverallScore,
            TotalCompressions = compressions.Count,
            AverageDepth = compressions.Count > 0 ? Math.Round(compressions.Average(c => c.DepthCm), 2) : 0m,
            AverageRate = compressions.Count > 0 ? (int)Math.Round(compressions.Average(c => (decimal)c.RateBpm)) : 0,
            RecoilRate = compressions.Count > 0
                ? Math.Round(compressions.Count(c => c.FullRecoil) / (decimal)compressions.Count * 100, 1)
                : 0m,
            DepthDistribution = depthDistribution,
            RateDistribution = rateDistribution,
            Compressions = compressions.Select(c => new
            {
                c.Timestamp, c.DepthCm, c.RateBpm, c.FullRecoil, c.QualityScore
            }),
            Suggestions = suggestions
        });
    }
}
