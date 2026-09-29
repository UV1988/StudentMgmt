using StudentManager.Api.Data;
using StudentManager.Api.Domain;
using StudentManager.Api.Dtos;

namespace StudentManager.Api.Services;

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponse>> GetAllAsync(CancellationToken ct);
    Task<StudentResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public class StudentService(
    IStudentRepository repository,
    TimeProvider clock,
    ILogger<StudentService> logger) : IStudentService
{
    public async Task<IReadOnlyList<StudentResponse>> GetAllAsync(CancellationToken ct)
    {
        var students = await repository.GetAllAsync(ct);
        return students.Select(ToResponse).ToList();
    }

    public async Task<StudentResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var student = await repository.GetByIdAsync(id, ct);
        return student is null ? null : ToResponse(student);
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone.Trim(),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime
        };

        await repository.AddAsync(student, ct);
        logger.LogInformation("Student {StudentId} created", student.Id);
        return ToResponse(student);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var deleted = await repository.DeleteAsync(id, ct);
        if (deleted)
        {
            logger.LogInformation("Student {StudentId} deleted", id);
        }
        return deleted;
    }

    private static StudentResponse ToResponse(Student s) =>
        new(s.Id, s.Name, s.DateOfBirth, s.Phone);
}
