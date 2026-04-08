namespace SimTrainer.Api.Simulation;

public class CprSimulator
{
    private readonly Random _random = new();
    private int _compressionCount;

    public (decimal DepthCm, int RateBpm, bool FullRecoil) GenerateCompression()
    {
        _compressionCount++;

        // Simulate realistic CPR with some variation and fatigue
        var fatigueFactor = Math.Min(1.0, _compressionCount / 200.0); // Fatigue sets in over time

        // Base depth around 5.5cm, drifts down with fatigue
        var depth = 5.5 - fatigueFactor * 0.8 + (_random.NextDouble() - 0.5) * 1.2;
        depth = Math.Max(2.0, Math.Min(7.5, depth));

        // Base rate around 110bpm, can drift with fatigue
        var rate = 110 + (_random.NextDouble() - 0.5) * 30 + fatigueFactor * 10;
        rate = Math.Max(60, Math.Min(160, rate));

        // Recoil quality decreases with fatigue
        var recoilChance = 0.9 - fatigueFactor * 0.3;
        var fullRecoil = _random.NextDouble() < recoilChance;

        return ((decimal)Math.Round(depth, 2), (int)Math.Round(rate), fullRecoil);
    }

    public void Reset()
    {
        _compressionCount = 0;
    }
}
