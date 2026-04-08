using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SimTrainer.Tests.Integration;

public class AnalyticsApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnalyticsApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string LearnerId, string SessionId)> CreateSessionWithCompressionsAsync()
    {
        var learnerResponse = await _client.PostAsJsonAsync("/api/learners", new { name = "Analytics Tester", role = "Paramedic" });
        var learner = await learnerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var learnerId = learner.GetProperty("id").GetString()!;

        var sessionResponse = await _client.PostAsJsonAsync("/api/sessions", new { learnerId, sessionType = "CprTraining" });
        var session = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>();
        var sessionId = session.GetProperty("id").GetString()!;

        // End the session to compute score
        await _client.PutAsync($"/api/sessions/{sessionId}/end", null);

        return (learnerId, sessionId);
    }

    [Fact]
    public async Task GetLearnerAnalytics_ExistingLearner_ReturnsAnalytics()
    {
        var (learnerId, _) = await CreateSessionWithCompressionsAsync();

        var response = await _client.GetAsync($"/api/analytics/learner/{learnerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(learnerId, body.GetProperty("learnerId").GetString());
        Assert.True(body.TryGetProperty("totalSessions", out _));
        Assert.True(body.TryGetProperty("suggestions", out _));
    }

    [Fact]
    public async Task GetLearnerAnalytics_NonexistentLearner_Returns404()
    {
        var response = await _client.GetAsync($"/api/analytics/learner/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionAnalytics_ExistingSession_ReturnsAnalytics()
    {
        var (_, sessionId) = await CreateSessionWithCompressionsAsync();

        var response = await _client.GetAsync($"/api/analytics/session/{sessionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("totalCompressions", out _));
        Assert.True(body.TryGetProperty("depthDistribution", out _));
        Assert.True(body.TryGetProperty("rateDistribution", out _));
        Assert.True(body.TryGetProperty("suggestions", out _));
    }

    [Fact]
    public async Task GetSessionAnalytics_NonexistentSession_Returns404()
    {
        var response = await _client.GetAsync($"/api/analytics/session/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
