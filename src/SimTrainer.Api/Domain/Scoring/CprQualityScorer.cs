namespace SimTrainer.Api.Domain.Scoring;

public static class CprQualityScorer
{
    private const decimal OptimalDepthMin = 5.0m;
    private const decimal OptimalDepthMax = 6.0m;
    private const int OptimalRateMin = 100;
    private const int OptimalRateMax = 120;

    public static decimal ScoreCompression(decimal depthCm, int rateBpm, bool fullRecoil)
    {
        var depthScore = ScoreDepth(depthCm);
        var rateScore = ScoreRate(rateBpm);
        var recoilScore = fullRecoil ? 100m : 0m;

        // AHA weighting: depth 40%, rate 40%, recoil 20%
        return Math.Round(depthScore * 0.4m + rateScore * 0.4m + recoilScore * 0.2m, 1);
    }

    public static decimal ScoreDepth(decimal depthCm)
    {
        if (depthCm >= OptimalDepthMin && depthCm <= OptimalDepthMax)
            return 100m;

        if (depthCm < OptimalDepthMin)
        {
            // Linear falloff below optimal range
            var deficit = OptimalDepthMin - depthCm;
            return Math.Max(0m, 100m - deficit * 40m);
        }

        // Above optimal range (risk of injury)
        var excess = depthCm - OptimalDepthMax;
        return Math.Max(0m, 100m - excess * 50m);
    }

    public static decimal ScoreRate(int rateBpm)
    {
        if (rateBpm >= OptimalRateMin && rateBpm <= OptimalRateMax)
            return 100m;

        if (rateBpm < OptimalRateMin)
        {
            var deficit = OptimalRateMin - rateBpm;
            return Math.Max(0m, 100m - deficit * 2.5m);
        }

        var excess = rateBpm - OptimalRateMax;
        return Math.Max(0m, 100m - excess * 2.5m);
    }

    public static string GetDepthFeedback(decimal depthCm)
    {
        if (depthCm < OptimalDepthMin) return "Push harder";
        if (depthCm > OptimalDepthMax) return "Too deep";
        return "Good depth";
    }

    public static string GetRateFeedback(int rateBpm)
    {
        if (rateBpm < OptimalRateMin) return "Push faster";
        if (rateBpm > OptimalRateMax) return "Slow down";
        return "Good rate";
    }
}
