using Microsoft.EntityFrameworkCore;
using StudentManager.Api.Domain;

namespace StudentManager.Api.Data;

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct);
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Student student, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public class StudentRepository(AppDbContext db) : IStudentRepository
{
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct) =>
        await db.Students.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Student student, CancellationToken ct)
    {
        db.Students.Add(student);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var rows = await db.Students.Where(s => s.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}
