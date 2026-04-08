namespace SimTrainer.Api.Domain.Entities;

public class ScenarioChange
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public string? PreviousScenario { get; set; }

    public TrainingSession Session { get; set; } = null!;
}
