using SimTrainer.Api.Domain.Scoring;

namespace SimTrainer.Tests.Domain;

public class CprQualityScorerTests
{
    [Theory]
    [InlineData(5.5, 110, true, 100.0)]   // Perfect compression
    [InlineData(5.0, 100, true, 100.0)]   // Lower boundary - still optimal
    [InlineData(6.0, 120, true, 100.0)]   // Upper boundary - still optimal
    public void ScoreCompression_OptimalValues_Returns100(decimal depth, int rate, bool recoil, decimal expected)
    {
        var score = CprQualityScorer.ScoreCompression(depth, rate, recoil);
        Assert.Equal(expected, score);
    }

    [Fact]
    public void ScoreCompression_NoRecoil_ReducesScore()
    {
        var withRecoil = CprQualityScorer.ScoreCompression(5.5m, 110, true);
        var withoutRecoil = CprQualityScorer.ScoreCompression(5.5m, 110, false);

        Assert.True(withRecoil > withoutRecoil);
        Assert.Equal(80.0m, withoutRecoil); // 100*0.4 + 100*0.4 + 0*0.2 = 80
    }

    [Fact]
    public void ScoreDepth_BelowOptimal_LinearFalloff()
    {
        var score = CprQualityScorer.ScoreDepth(4.0m);
        // deficit = 5.0 - 4.0 = 1.0, score = 100 - 1.0*40 = 60
        Assert.Equal(60m, score);
    }

    [Fact]
    public void ScoreDepth_AboveOptimal_SteepFalloff()
    {
        var score = CprQualityScorer.ScoreDepth(7.0m);
        // excess = 7.0 - 6.0 = 1.0, score = 100 - 1.0*50 = 50
        Assert.Equal(50m, score);
    }

    [Fact]
    public void ScoreDepth_Zero_ReturnsZero()
    {
        var score = CprQualityScorer.ScoreDepth(0m);
        Assert.Equal(0m, score);
    }

    [Fact]
    public void ScoreRate_BelowOptimal_GradualFalloff()
    {
        var score = CprQualityScorer.ScoreRate(80);
        // deficit = 100 - 80 = 20, score = 100 - 20*2.5 = 50
        Assert.Equal(50m, score);
    }

    [Fact]
    public void ScoreRate_AboveOptimal_GradualFalloff()
    {
        var score = CprQualityScorer.ScoreRate(140);
        // excess = 140 - 120 = 20, score = 100 - 20*2.5 = 50
        Assert.Equal(50m, score);
    }

    [Fact]
    public void ScoreRate_Zero_ReturnsZero()
    {
        var score = CprQualityScorer.ScoreRate(0);
        Assert.Equal(0m, score);
    }

    [Fact]
    public void ScoreRate_ExactlyAtBoundary_Returns100()
    {
        Assert.Equal(100m, CprQualityScorer.ScoreRate(100));
        Assert.Equal(100m, CprQualityScorer.ScoreRate(120));
    }

    [Fact]
    public void GetDepthFeedback_Shallow_ReturnsPushHarder()
    {
        Assert.Equal("Push harder", CprQualityScorer.GetDepthFeedback(4.0m));
    }

    [Fact]
    public void GetDepthFeedback_TooDeep_ReturnsTooDeep()
    {
        Assert.Equal("Too deep", CprQualityScorer.GetDepthFeedback(6.5m));
    }

    [Fact]
    public void GetDepthFeedback_Optimal_ReturnsGood()
    {
        Assert.Equal("Good depth", CprQualityScorer.GetDepthFeedback(5.5m));
    }

    [Fact]
    public void GetRateFeedback_Slow_ReturnsPushFaster()
    {
        Assert.Equal("Push faster", CprQualityScorer.GetRateFeedback(80));
    }

    [Fact]
    public void GetRateFeedback_Fast_ReturnsSlowDown()
    {
        Assert.Equal("Slow down", CprQualityScorer.GetRateFeedback(140));
    }

    [Fact]
    public void GetRateFeedback_Optimal_ReturnsGoodRate()
    {
        Assert.Equal("Good rate", CprQualityScorer.GetRateFeedback(110));
    }
}
