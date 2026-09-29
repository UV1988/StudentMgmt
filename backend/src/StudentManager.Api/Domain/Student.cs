namespace StudentManager.Api.Domain;

/// <summary>The persisted student record. Kept separate from the API DTOs on purpose.</summary>
public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
