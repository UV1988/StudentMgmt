using StudentManager.Api.Data;
using StudentManager.Api.Domain;

namespace StudentManager.Tests;

/// <summary>A clock that always returns the same instant, so date-based rules are deterministic.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

/// <summary>In-memory stand-in for the EF repository, used in service unit tests.</summary>
public sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Items { get; } = [];

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Student>>(Items.OrderBy(s => s.Name).ToList());

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

    public Task AddAsync(Student student, CancellationToken ct)
    {
        Items.Add(student);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.RemoveAll(s => s.Id == id) > 0);
}
