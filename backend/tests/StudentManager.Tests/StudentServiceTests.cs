using Microsoft.Extensions.Logging.Abstractions;
using StudentManager.Api.Dtos;
using StudentManager.Api.Services;

namespace StudentManager.Tests;

public class StudentServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private readonly FakeStudentRepository _repository = new();
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _service = new StudentService(_repository, new FixedTimeProvider(Now), NullLogger<StudentService>.Instance);
    }

    [Fact]
    public async Task Create_trims_input_and_stamps_created_time()
    {
        var response = await _service.CreateAsync(
            new CreateStudentRequest("  Asha Rao  ", new DateOnly(2000, 1, 15), " 9876543210 "),
            CancellationToken.None);

        Assert.Equal("Asha Rao", response.Name);
        Assert.Equal("9876543210", response.Phone);

        var stored = Assert.Single(_repository.Items);
        Assert.Equal(response.Id, stored.Id);
        Assert.Equal(Now.UtcDateTime, stored.CreatedAtUtc);
    }

    [Fact]
    public async Task GetAll_returns_students_ordered_by_name()
    {
        await _service.CreateAsync(new CreateStudentRequest("Zoya", new DateOnly(2001, 5, 5), "9876543210"), CancellationToken.None);
        await _service.CreateAsync(new CreateStudentRequest("Aman", new DateOnly(2002, 6, 6), "9876543211"), CancellationToken.None);

        var all = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(["Aman", "Zoya"], all.Select(s => s.Name).ToArray());
    }

    [Fact]
    public async Task Delete_returns_true_when_student_exists()
    {
        var created = await _service.CreateAsync(
            new CreateStudentRequest("Asha", new DateOnly(2000, 1, 15), "9876543210"), CancellationToken.None);

        Assert.True(await _service.DeleteAsync(created.Id, CancellationToken.None));
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Delete_returns_false_when_student_is_missing() =>
        Assert.False(await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
}
