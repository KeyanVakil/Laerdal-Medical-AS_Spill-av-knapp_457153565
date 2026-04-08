namespace SimTrainer.Api.Domain.Entities;

public class VitalSnapshot
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int HeartRate { get; set; }
    public int SpO2 { get; set; }
    public int SystolicBp { get; set; }
    public int DiastolicBp { get; set; }
    public int RespiratoryRate { get; set; }

    public TrainingSession Session { get; set; } = null!;
}
