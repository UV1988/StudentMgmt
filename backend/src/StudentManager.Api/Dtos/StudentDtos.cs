namespace StudentManager.Api.Dtos;

public record CreateStudentRequest(string Name, DateOnly DateOfBirth, string Phone);

public record StudentResponse(Guid Id, string Name, DateOnly DateOfBirth, string Phone);
