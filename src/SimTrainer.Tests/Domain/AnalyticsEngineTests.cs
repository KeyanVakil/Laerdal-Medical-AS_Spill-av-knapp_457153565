using SimTrainer.Api.Domain.Entities;
using SimTrainer.Api.Domain.Scoring;

namespace SimTrainer.Tests.Domain;

public class AnalyticsEngineTests
{
    [Fact]
    public void GenerateSuggestions_EmptyList_ReturnsEmpty()
    {
        var suggestions = AnalyticsEngine.GenerateSuggestions(new List<CompressionEvent>());
        Assert.Empty(suggestions);
    }

    [Fact]
    public void GenerateSuggestions_AllOptimal_ReturnsExcellent()
    {
        var compressions = Enumerable.Range(0, 20).Select(_ => new CompressionEvent
        {
            DepthCm = 5.5m,
            RateBpm = 110,
            FullRecoil = true,
            QualityScore = 100m
        }).ToList();

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Single(suggestions);
        Assert.Contains("Excellent", suggestions[0]);
    }

    [Fact]
    public void GenerateSuggestions_ShallowDepth_SuggestsPushHarder()
    {
        var compressions = Enumerable.Range(0, 10).Select(_ => new CompressionEvent
        {
            DepthCm = 4.0m,
            RateBpm = 110,
            FullRecoil = true,
            QualityScore = 80m
        }).ToList();

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Contains(suggestions, s => s.Contains("below 5cm"));
    }

    [Fact]
    public void GenerateSuggestions_TooDeep_SuggestsEaseUp()
    {
        var compressions = Enumerable.Range(0, 10).Select(_ => new CompressionEvent
        {
            DepthCm = 6.5m,
            RateBpm = 110,
            FullRecoil = true,
            QualityScore = 80m
        }).ToList();

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Contains(suggestions, s => s.Contains("too deep"));
    }

    [Fact]
    public void GenerateSuggestions_FastRate_SuggestsSlowDown()
    {
        var compressions = Enumerable.Range(0, 10).Select(_ => new CompressionEvent
        {
            DepthCm = 5.5m,
            RateBpm = 140,
            FullRecoil = true,
            QualityScore = 80m
        }).ToList();

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Contains(suggestions, s => s.Contains("too fast"));
    }

    [Fact]
    public void GenerateSuggestions_PoorRecoil_SuggestsImprovement()
    {
        var compressions = Enumerable.Range(0, 10).Select(_ => new CompressionEvent
        {
            DepthCm = 5.5m,
            RateBpm = 110,
            FullRecoil = false,
            QualityScore = 80m
        }).ToList();

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Contains(suggestions, s => s.Contains("recoil"));
    }

    [Fact]
    public void GenerateSuggestions_RateDriftsUp_DetectsDrift()
    {
        var compressions = new List<CompressionEvent>();
        for (int i = 0; i < 20; i++)
        {
            compressions.Add(new CompressionEvent
            {
                DepthCm = 5.5m,
                RateBpm = 100 + i * 3,  // Drifts from 100 to 157
                FullRecoil = true,
                QualityScore = 80m
            });
        }

        var suggestions = AnalyticsEngine.GenerateSuggestions(compressions);
        Assert.Contains(suggestions, s => s.Contains("drift upward"));
    }

    [Fact]
    public void DetermineTrend_Improving_ReturnsImproving()
    {
        var scores = new List<decimal> { 60, 65, 70, 75, 80, 85, 90 };
        Assert.Equal("improving", AnalyticsEngine.DetermineTrend(scores));
    }

    [Fact]
    public void DetermineTrend_Declining_ReturnsDeclining()
    {
        var scores = new List<decimal> { 90, 85, 80, 75, 70, 65, 60 };
        Assert.Equal("declining", AnalyticsEngine.DetermineTrend(scores));
    }

    [Fact]
    public void DetermineTrend_Stable_ReturnsStable()
    {
        var scores = new List<decimal> { 80, 81, 79, 80, 81, 80 };
        Assert.Equal("stable", AnalyticsEngine.DetermineTrend(scores));
    }

    [Fact]
    public void DetermineTrend_SingleScore_ReturnsInsufficientData()
    {
        Assert.Equal("insufficient_data", AnalyticsEngine.DetermineTrend(new List<decimal> { 80 }));
    }

    [Fact]
    public void ComputeOverallScore_Empty_ReturnsZero()
    {
        Assert.Equal(0m, AnalyticsEngine.ComputeOverallScore(new List<CompressionEvent>()));
    }

    [Fact]
    public void ComputeOverallScore_MultipleEvents_ReturnsAverage()
    {
        var events = new List<CompressionEvent>
        {
            new() { QualityScore = 80 },
            new() { QualityScore = 90 },
            new() { QualityScore = 100 }
        };
        Assert.Equal(90.0m, AnalyticsEngine.ComputeOverallScore(events));
    }
}
