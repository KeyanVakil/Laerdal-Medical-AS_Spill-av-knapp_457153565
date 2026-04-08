using SimTrainer.Api.Domain.Enums;

namespace SimTrainer.Api.Domain.Entities;

public class TrainingSession
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public SessionType SessionType { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public decimal? OverallScore { get; set; }

    public Learner Learner { get; set; } = null!;
    public List<CompressionEvent> Compressions { get; set; } = new();
    public List<VitalSnapshot> Vitals { get; set; } = new();
    public List<ScenarioChange> ScenarioChanges { get; set; } = new();
}
