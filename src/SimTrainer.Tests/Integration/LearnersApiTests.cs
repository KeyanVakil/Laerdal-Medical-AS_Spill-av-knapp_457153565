using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SimTrainer.Tests.Integration;

public class LearnersApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LearnersApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLearners_Empty_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/learners");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var learners = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions);
        Assert.NotNull(learners);
    }

    [Fact]
    public async Task CreateLearner_ValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/learners", new { name = "Test User", role = "Nurse" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Test User", body.GetProperty("name").GetString());
        Assert.Equal("Nurse", body.GetProperty("role").GetString());
        Assert.NotEqual(Guid.Empty.ToString(), body.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetLearnerById_Existing_ReturnsLearner()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/learners", new { name = "Lookup Test", role = "Doctor" });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/learners/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Lookup Test", body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetLearnerById_Nonexistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/learners/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLearner_ValidData_ReturnsUpdated()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/learners", new { name = "Before", role = "Student" });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var response = await _client.PutAsJsonAsync($"/api/learners/{id}", new { name = "After", role = "Resident" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("After", body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task DeleteLearner_Existing_Returns204()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/learners", new { name = "ToDelete", role = "Nurse" });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/api/learners/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
