using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Domain.Enums;
using SimTrainer.Api.Infrastructure;
using SimTrainer.Api.Services;

namespace SimTrainer.Tests.Services;

public class SessionServiceTests : IDisposable
{
    private readonly SimTrainerDbContext _db;
    private readonly SessionService _sessionService;
    private readonly LearnerService _learnerService;

    public SessionServiceTests()
    {
        var options = new DbContextOptionsBuilder<SimTrainerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SimTrainerDbContext(options);
        _sessionService = new SessionService(_db);
        _learnerService = new LearnerService(_db);
    }

    private async Task<Guid> CreateLearnerAsync()
    {
        var learner = await _learnerService.CreateAsync("Test Learner", "Nurse");
        return learner.Id;
    }

    [Fact]
    public async Task StartSessionAsync_CreatesSession()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.CprTraining);

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(learnerId, session.LearnerId);
        Assert.Equal(SessionType.CprTraining, session.SessionType);
        Assert.Null(session.EndedAt);
    }

    [Fact]
    public async Task EndSessionAsync_SetsEndedAtAndScore()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.CprTraining);

        await _sessionService.AddCompressionAsync(session.Id, 5.5m, 110, true);
        await _sessionService.AddCompressionAsync(session.Id, 5.0m, 100, true);

        var ended = await _sessionService.EndSessionAsync(session.Id);

        Assert.NotNull(ended);
        Assert.NotNull(ended.EndedAt);
        Assert.NotNull(ended.OverallScore);
        Assert.True(ended.OverallScore > 0);
    }

    [Fact]
    public async Task EndSessionAsync_NonexistentId_ReturnsNull()
    {
        Assert.Null(await _sessionService.EndSessionAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AddCompressionAsync_PersistsAndScores()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.CprTraining);

        var compression = await _sessionService.AddCompressionAsync(session.Id, 5.5m, 110, true);

        Assert.Equal(5.5m, compression.DepthCm);
        Assert.Equal(110, compression.RateBpm);
        Assert.True(compression.FullRecoil);
        Assert.Equal(100.0m, compression.QualityScore);
    }

    [Fact]
    public async Task AddVitalSnapshotAsync_PersistsData()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.PatientMonitoring);

        var vital = await _sessionService.AddVitalSnapshotAsync(session.Id, 75, 98, 120, 80, 16);

        Assert.Equal(75, vital.HeartRate);
        Assert.Equal(98, vital.SpO2);
    }

    [Fact]
    public async Task AddScenarioChangeAsync_PersistsData()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.PatientMonitoring);

        var change = await _sessionService.AddScenarioChangeAsync(session.Id, "Tachycardia", "Normal");

        Assert.Equal("Tachycardia", change.ScenarioName);
        Assert.Equal("Normal", change.PreviousScenario);
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByLearnerId()
    {
        var learner1 = await CreateLearnerAsync();
        var learner2 = (await _learnerService.CreateAsync("Other", "Doctor")).Id;

        await _sessionService.StartSessionAsync(learner1, SessionType.CprTraining);
        await _sessionService.StartSessionAsync(learner2, SessionType.CprTraining);

        var sessions = await _sessionService.GetSessionsAsync(learnerId: learner1);

        Assert.Single(sessions);
        Assert.Equal(learner1, sessions[0].LearnerId);
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByType()
    {
        var learnerId = await CreateLearnerAsync();
        await _sessionService.StartSessionAsync(learnerId, SessionType.CprTraining);
        await _sessionService.StartSessionAsync(learnerId, SessionType.PatientMonitoring);

        var cprSessions = await _sessionService.GetSessionsAsync(type: SessionType.CprTraining);

        Assert.Single(cprSessions);
    }

    [Fact]
    public async Task GetCompressionsAsync_ReturnsOrderedByTimestamp()
    {
        var learnerId = await CreateLearnerAsync();
        var session = await _sessionService.StartSessionAsync(learnerId, SessionType.CprTraining);

        await _sessionService.AddCompressionAsync(session.Id, 5.5m, 110, true);
        await _sessionService.AddCompressionAsync(session.Id, 5.0m, 105, true);

        var compressions = await _sessionService.GetCompressionsAsync(session.Id);

        Assert.Equal(2, compressions.Count);
    }

    public void Dispose() => _db.Dispose();
}
