using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Domain.Entities;
using SimTrainer.Api.Infrastructure;

namespace SimTrainer.Api.Services;

public class LearnerService
{
    private readonly SimTrainerDbContext _db;

    public LearnerService(SimTrainerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Learner>> GetAllAsync()
    {
        return await _db.Learners
            .Include(l => l.Sessions)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Learner?> GetByIdAsync(Guid id)
    {
        return await _db.Learners
            .Include(l => l.Sessions)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Learner> CreateAsync(string name, string role)
    {
        var learner = new Learner
        {
            Id = Guid.NewGuid(),
            Name = name,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        _db.Learners.Add(learner);
        await _db.SaveChangesAsync();
        return learner;
    }

    public async Task<Learner?> UpdateAsync(Guid id, string name, string role)
    {
        var learner = await _db.Learners.FindAsync(id);
        if (learner == null) return null;

        learner.Name = name;
        learner.Role = role;
        await _db.SaveChangesAsync();
        return learner;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var learner = await _db.Learners.FindAsync(id);
        if (learner == null) return false;

        _db.Learners.Remove(learner);
        await _db.SaveChangesAsync();
        return true;
    }
}
