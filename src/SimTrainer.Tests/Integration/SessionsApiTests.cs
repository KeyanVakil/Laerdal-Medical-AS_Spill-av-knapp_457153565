using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SimTrainer.Tests.Integration;

public class SessionsApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SessionsApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> CreateLearnerAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/learners", new { name = "Session Tester", role = "Nurse" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task StartSession_ValidLearner_Returns201()
    {
        var learnerId = await CreateLearnerAsync();

        var response = await _client.PostAsJsonAsync("/api/sessions", new
        {
            learnerId,
            sessionType = "CprTraining"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CprTraining", body.GetProperty("sessionType").GetString());
    }

    [Fact]
    public async Task EndSession_ActiveSession_ReturnsWithEndedAt()
    {
        var learnerId = await CreateLearnerAsync();
        var startResponse = await _client.PostAsJsonAsync("/api/sessions", new
        {
            learnerId,
            sessionType = "CprTraining"
        });
        var session = await startResponse.Content.ReadFromJsonAsync<JsonElement>();
        var sessionId = session.GetProperty("id").GetString();

        var endResponse = await _client.PutAsync($"/api/sessions/{sessionId}/end", null);

        Assert.Equal(HttpStatusCode.OK, endResponse.StatusCode);
        var ended = await endResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual(JsonValueKind.Null, ended.GetProperty("endedAt").ValueKind);
    }

    [Fact]
    public async Task GetSessions_FilterByLearnerId_ReturnsFiltered()
    {
        var learnerId = await CreateLearnerAsync();
        await _client.PostAsJsonAsync("/api/sessions", new { learnerId, sessionType = "CprTraining" });

        var response = await _client.GetAsync($"/api/sessions?learnerId={learnerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(body);
        Assert.All(body, s => Assert.Equal(learnerId, s.GetProperty("learnerId").GetString()));
    }

    [Fact]
    public async Task GetSession_WithDetails_ReturnsFullData()
    {
        var learnerId = await CreateLearnerAsync();
        var startResponse = await _client.PostAsJsonAsync("/api/sessions", new { learnerId, sessionType = "CprTraining" });
        var session = await startResponse.Content.ReadFromJsonAsync<JsonElement>();
        var sessionId = session.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/sessions/{sessionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("compressions", out _));
        Assert.True(body.TryGetProperty("vitals", out _));
        Assert.True(body.TryGetProperty("scenarioChanges", out _));
    }
}
