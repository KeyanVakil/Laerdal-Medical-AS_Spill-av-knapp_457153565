using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Infrastructure;
using SimTrainer.Api.Services;
using Xunit;

namespace SimTrainer.Tests.Services;

public class LearnerServiceTests : IDisposable
{
    private readonly SimTrainerDbContext _db;
    private readonly LearnerService _service;

    public LearnerServiceTests()
    {
        var options = new DbContextOptionsBuilder<SimTrainerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SimTrainerDbContext(options);
        _service = new LearnerService(_db);
    }

    [Fact]
    public async Task CreateAsync_ReturnsLearnerWithId()
    {
        var learner = await _service.CreateAsync("Anna Olsen", "Nurse");

        Assert.NotEqual(Guid.Empty, learner.Id);
        Assert.Equal("Anna Olsen", learner.Name);
        Assert.Equal("Nurse", learner.Role);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLearners()
    {
        await _service.CreateAsync("Alice", "Nurse");
        await _service.CreateAsync("Bob", "Paramedic");

        var all = await _service.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsLearner()
    {
        var created = await _service.CreateAsync("Charlie", "Doctor");
        var found = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal("Charlie", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonexistentId_ReturnsNull()
    {
        var found = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_ExistingLearner_UpdatesFields()
    {
        var created = await _service.CreateAsync("Dave", "Student");
        var updated = await _service.UpdateAsync(created.Id, "David", "Resident");

        Assert.NotNull(updated);
        Assert.Equal("David", updated.Name);
        Assert.Equal("Resident", updated.Role);
    }

    [Fact]
    public async Task UpdateAsync_NonexistentId_ReturnsNull()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), "Nobody", "None");
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingLearner_ReturnsTrue()
    {
        var created = await _service.CreateAsync("Eve", "Nurse");
        var deleted = await _service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonexistentId_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    public void Dispose() => _db.Dispose();
}
