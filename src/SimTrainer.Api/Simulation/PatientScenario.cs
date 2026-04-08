namespace SimTrainer.Api.Simulation;

public record VitalRanges(
    int HeartRateMin, int HeartRateMax,
    int SpO2Min, int SpO2Max,
    int SystolicBp, int DiastolicBp,
    int RespRateMin, int RespRateMax);

public static class PatientScenario
{
    public static readonly Dictionary<string, VitalRanges> Scenarios = new()
    {
        ["Normal"] = new(60, 100, 95, 100, 120, 80, 12, 20),
        ["Tachycardia"] = new(130, 160, 92, 96, 100, 65, 18, 24),
        ["Bradycardia"] = new(35, 50, 94, 98, 90, 60, 10, 14),
        ["Hypoxia"] = new(110, 130, 75, 88, 130, 85, 24, 32),
        ["Cardiac Arrest"] = new(0, 0, 0, 0, 0, 0, 0, 0),
    };

    public static VitalRanges GetScenario(string name)
    {
        return Scenarios.TryGetValue(name, out var ranges)
            ? ranges
            : Scenarios["Normal"];
    }
}
