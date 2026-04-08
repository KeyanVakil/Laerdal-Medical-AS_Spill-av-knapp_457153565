namespace SimTrainer.Api.Domain.Entities;

public class CompressionEvent
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal DepthCm { get; set; }
    public int RateBpm { get; set; }
    public bool FullRecoil { get; set; }
    public decimal QualityScore { get; set; }

    public TrainingSession Session { get; set; } = null!;
}
