namespace SimTrainer.Api.Domain.Entities;

public class Learner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TrainingSession> Sessions { get; set; } = new();
}
