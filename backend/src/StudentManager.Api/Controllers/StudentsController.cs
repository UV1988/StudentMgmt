using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Api.Dtos;
using StudentManager.Api.Services;

namespace StudentManager.Api.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public class StudentsController(
    IStudentService service,
    IValidator<CreateStudentRequest> validator) : ControllerBase
{
    /// <summary>List all students, ordered by name.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StudentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    /// <summary>Get one student by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var student = await service.GetByIdAsync(id, ct);
        return student is null ? NotFound() : Ok(student);
    }

    /// <summary>Add a student.</summary>
    [HttpPost]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Delete a student.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
