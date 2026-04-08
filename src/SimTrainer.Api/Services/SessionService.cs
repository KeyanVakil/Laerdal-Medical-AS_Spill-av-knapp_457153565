using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Domain.Entities;
using SimTrainer.Api.Domain.Enums;
using SimTrainer.Api.Domain.Scoring;
using SimTrainer.Api.Infrastructure;

namespace SimTrainer.Api.Services;

public class SessionService
{
    private readonly SimTrainerDbContext _db;

    public SessionService(SimTrainerDbContext db)
    {
        _db = db;
    }

    public async Task<List<TrainingSession>> GetSessionsAsync(
        Guid? learnerId = null, SessionType? type = null,
        DateTime? from = null, DateTime? to = null)
    {
        var query = _db.TrainingSessions
            .Include(s => s.Learner)
            .AsQueryable();

        if (learnerId.HasValue)
            query = query.Where(s => s.LearnerId == learnerId.Value);
        if (type.HasValue)
            query = query.Where(s => s.SessionType == type.Value);
        if (from.HasValue)
            query = query.Where(s => s.StartedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.StartedAt <= to.Value);

        return await query.OrderByDescending(s => s.StartedAt).ToListAsync();
    }

    public async Task<TrainingSession?> GetSessionAsync(Guid id)
    {
        return await _db.TrainingSessions
            .Include(s => s.Learner)
            .Include(s => s.Compressions.OrderBy(c => c.Timestamp))
            .Include(s => s.Vitals.OrderBy(v => v.Timestamp))
            .Include(s => s.ScenarioChanges.OrderBy(sc => sc.Timestamp))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<TrainingSession> StartSessionAsync(Guid learnerId, SessionType sessionType)
    {
        var session = new TrainingSession
        {
            Id = Guid.NewGuid(),
            LearnerId = learnerId,
            SessionType = sessionType,
            StartedAt = DateTime.UtcNow
        };
        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<TrainingSession?> EndSessionAsync(Guid id)
    {
        var session = await _db.TrainingSessions
            .Include(s => s.Compressions)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null) return null;

        session.EndedAt = DateTime.UtcNow;

        if (session.SessionType == SessionType.CprTraining && session.Compressions.Count > 0)
        {
            session.OverallScore = AnalyticsEngine.ComputeOverallScore(session.Compressions);
        }

        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<CompressionEvent> AddCompressionAsync(Guid sessionId, decimal depthCm, int rateBpm, bool fullRecoil)
    {
        var qualityScore = CprQualityScorer.ScoreCompression(depthCm, rateBpm, fullRecoil);

        var compression = new CompressionEvent
        {
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow,
            DepthCm = depthCm,
            RateBpm = rateBpm,
            FullRecoil = fullRecoil,
            QualityScore = qualityScore
        };

        _db.CompressionEvents.Add(compression);
        await _db.SaveChangesAsync();
        return compression;
    }

    public async Task<VitalSnapshot> AddVitalSnapshotAsync(Guid sessionId, int heartRate, int spO2, int systolicBp, int diastolicBp, int respRate)
    {
        var vital = new VitalSnapshot
        {
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow,
            HeartRate = heartRate,
            SpO2 = spO2,
            SystolicBp = systolicBp,
            DiastolicBp = diastolicBp,
            RespiratoryRate = respRate
        };

        _db.VitalSnapshots.Add(vital);
        await _db.SaveChangesAsync();
        return vital;
    }

    public async Task<ScenarioChange> AddScenarioChangeAsync(Guid sessionId, string scenarioName, string? previousScenario)
    {
        var change = new ScenarioChange
        {
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow,
            ScenarioName = scenarioName,
            PreviousScenario = previousScenario
        };

        _db.ScenarioChanges.Add(change);
        await _db.SaveChangesAsync();
        return change;
    }

    public async Task<List<CompressionEvent>> GetCompressionsAsync(Guid sessionId)
    {
        return await _db.CompressionEvents
            .Where(c => c.SessionId == sessionId)
            .OrderBy(c => c.Timestamp)
            .ToListAsync();
    }

    public async Task<List<VitalSnapshot>> GetVitalsAsync(Guid sessionId)
    {
        return await _db.VitalSnapshots
            .Where(v => v.SessionId == sessionId)
            .OrderBy(v => v.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ScenarioChange>> GetScenarioChangesAsync(Guid sessionId)
    {
        return await _db.ScenarioChanges
            .Where(sc => sc.SessionId == sessionId)
            .OrderBy(sc => sc.Timestamp)
            .ToListAsync();
    }
}
