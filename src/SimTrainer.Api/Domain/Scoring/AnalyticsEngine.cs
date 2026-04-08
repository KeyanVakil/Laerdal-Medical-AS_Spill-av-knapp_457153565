using SimTrainer.Api.Domain.Entities;

namespace SimTrainer.Api.Domain.Scoring;

public static class AnalyticsEngine
{
    public static List<string> GenerateSuggestions(IReadOnlyList<CompressionEvent> compressions)
    {
        var suggestions = new List<string>();
        if (compressions.Count == 0) return suggestions;

        var avgDepth = compressions.Average(c => c.DepthCm);
        var avgRate = compressions.Average(c => (decimal)c.RateBpm);
        var recoilRate = compressions.Count(c => c.FullRecoil) / (decimal)compressions.Count * 100;

        if (avgDepth < 5.0m)
            suggestions.Add($"Compression depth is consistently below 5cm (avg: {avgDepth:F1}cm) — focus on pushing harder");
        else if (avgDepth > 6.0m)
            suggestions.Add($"Compression depth is too deep (avg: {avgDepth:F1}cm) — ease up slightly to avoid injury risk");

        if (avgRate < 100)
            suggestions.Add($"Compression rate is below target (avg: {avgRate:F0} bpm) — aim for 100-120 compressions per minute");
        else if (avgRate > 120)
            suggestions.Add($"Compression rate is too fast (avg: {avgRate:F0} bpm) — slow down to maintain quality");

        if (recoilRate < 80)
            suggestions.Add($"Full chest recoil achieved only {recoilRate:F0}% of the time — allow complete chest rise between compressions");

        // Check for drift: compare first half vs second half
        if (compressions.Count >= 10)
        {
            var midpoint = compressions.Count / 2;
            var firstHalfRate = compressions.Take(midpoint).Average(c => (decimal)c.RateBpm);
            var secondHalfRate = compressions.Skip(midpoint).Average(c => (decimal)c.RateBpm);

            if (secondHalfRate - firstHalfRate > 10)
                suggestions.Add("Rate tends to drift upward during sessions — practice maintaining a steady pace");
            else if (firstHalfRate - secondHalfRate > 10)
                suggestions.Add("Rate tends to drop later in sessions — work on endurance to maintain consistent tempo");

            var firstHalfDepth = compressions.Take(midpoint).Average(c => c.DepthCm);
            var secondHalfDepth = compressions.Skip(midpoint).Average(c => c.DepthCm);

            if (firstHalfDepth - secondHalfDepth > 0.5m)
                suggestions.Add("Compression depth decreases over time — fatigue may be affecting performance");
        }

        if (suggestions.Count == 0)
            suggestions.Add("Excellent performance! All metrics are within AHA guidelines");

        return suggestions;
    }

    public static string DetermineTrend(IReadOnlyList<decimal> scores)
    {
        if (scores.Count < 2) return "insufficient_data";

        // Simple linear trend: compare average of recent third vs first third
        var third = Math.Max(1, scores.Count / 3);
        var firstAvg = scores.Take(third).Average();
        var lastAvg = scores.Skip(scores.Count - third).Average();

        var diff = lastAvg - firstAvg;
        if (diff > 5) return "improving";
        if (diff < -5) return "declining";
        return "stable";
    }

    public static decimal ComputeOverallScore(IReadOnlyList<CompressionEvent> compressions)
    {
        if (compressions.Count == 0) return 0;
        return Math.Round(compressions.Average(c => c.QualityScore), 1);
    }
}
