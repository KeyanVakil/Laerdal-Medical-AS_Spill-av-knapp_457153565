namespace SimTrainer.Api.Simulation;

public class VitalSignsGenerator
{
    private readonly Random _random = new();
    private VitalRanges _currentTarget;
    private VitalRanges _previousTarget;
    private DateTime _transitionStart;
    private TimeSpan _transitionDuration = TimeSpan.FromSeconds(8);
    private bool _isTransitioning;

    // Current smoothed values
    private double _heartRate;
    private double _spO2;
    private double _systolicBp;
    private double _diastolicBp;
    private double _respRate;

    public string CurrentScenario { get; private set; } = "Normal";

    public VitalSignsGenerator()
    {
        _currentTarget = PatientScenario.GetScenario("Normal");
        _previousTarget = _currentTarget;
        InitializeFromRanges(_currentTarget);
    }

    public void SetScenario(string scenarioName)
    {
        var newTarget = PatientScenario.GetScenario(scenarioName);
        _previousTarget = _currentTarget;
        _currentTarget = newTarget;
        _transitionStart = DateTime.UtcNow;
        _isTransitioning = true;
        CurrentScenario = scenarioName;
    }

    public (int HeartRate, int SpO2, int SystolicBp, int DiastolicBp, int RespRate) GenerateVitals()
    {
        var target = GetEffectiveTarget();

        // Smoothly move toward target with noise
        _heartRate = Approach(_heartRate, MidRange(target.HeartRateMin, target.HeartRateMax), 0.15) + Noise(2);
        _spO2 = Approach(_spO2, MidRange(target.SpO2Min, target.SpO2Max), 0.15) + Noise(1);
        _systolicBp = Approach(_systolicBp, target.SystolicBp, 0.12) + Noise(3);
        _diastolicBp = Approach(_diastolicBp, target.DiastolicBp, 0.12) + Noise(2);
        _respRate = Approach(_respRate, MidRange(target.RespRateMin, target.RespRateMax), 0.15) + Noise(1);

        return (
            Clamp(_heartRate, 0, 250),
            Clamp(_spO2, 0, 100),
            Clamp(_systolicBp, 0, 250),
            Clamp(_diastolicBp, 0, 200),
            Clamp(_respRate, 0, 60)
        );
    }

    private VitalRanges GetEffectiveTarget()
    {
        if (!_isTransitioning) return _currentTarget;

        var elapsed = DateTime.UtcNow - _transitionStart;
        if (elapsed >= _transitionDuration)
        {
            _isTransitioning = false;
            return _currentTarget;
        }

        // Linear interpolation factor
        var t = elapsed / _transitionDuration;
        return Interpolate(_previousTarget, _currentTarget, t);
    }

    private static VitalRanges Interpolate(VitalRanges from, VitalRanges to, double t)
    {
        return new VitalRanges(
            Lerp(from.HeartRateMin, to.HeartRateMin, t),
            Lerp(from.HeartRateMax, to.HeartRateMax, t),
            Lerp(from.SpO2Min, to.SpO2Min, t),
            Lerp(from.SpO2Max, to.SpO2Max, t),
            Lerp(from.SystolicBp, to.SystolicBp, t),
            Lerp(from.DiastolicBp, to.DiastolicBp, t),
            Lerp(from.RespRateMin, to.RespRateMin, t),
            Lerp(from.RespRateMax, to.RespRateMax, t)
        );
    }

    private static int Lerp(int a, int b, double t) => (int)(a + (b - a) * t);
    private static double MidRange(int min, int max) => (min + max) / 2.0;

    private double Approach(double current, double target, double rate)
    {
        return current + (target - current) * rate;
    }

    private double Noise(double amplitude) => (_random.NextDouble() - 0.5) * 2 * amplitude;

    private static int Clamp(double value, int min, int max) =>
        (int)Math.Round(Math.Clamp(value, min, max));

    private void InitializeFromRanges(VitalRanges ranges)
    {
        _heartRate = MidRange(ranges.HeartRateMin, ranges.HeartRateMax);
        _spO2 = MidRange(ranges.SpO2Min, ranges.SpO2Max);
        _systolicBp = ranges.SystolicBp;
        _diastolicBp = ranges.DiastolicBp;
        _respRate = MidRange(ranges.RespRateMin, ranges.RespRateMax);
    }
}
