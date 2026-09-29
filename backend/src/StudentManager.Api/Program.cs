using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentManager.Api.Data;
using StudentManager.Api.Infrastructure;
using StudentManager.Api.Services;
using StudentManager.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Data
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")));

// Application services
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentRequestValidator>();

// Web
// FluentValidation is the single source of validation rules, so switch off MVC's implicit [Required]
// on non-nullable reference types (it would otherwise answer with generic messages first).
builder.Services.AddControllers(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Creates the SQLite file and table on first run. See README ("Next steps") for moving to EF migrations.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Makes the entry point visible to WebApplicationFactory in the test project.
public partial class Program;
