using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using StudentManager.Api.Dtos;

namespace StudentManager.Tests;

/// <summary>Boots the real API against a throwaway SQLite file for each test class.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"students-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_dbPath}"
            }));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" })
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}

public class StudentsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object NewStudent(string name) =>
        new { name, dateOfBirth = "2000-01-15", phone = "+91 98765 43210" };

    [Fact]
    public async Task Post_creates_student_and_it_appears_in_the_list()
    {
        var response = await _client.PostAsJsonAsync("/api/students", NewStudent("Api Create Test"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(created);

        var list = await _client.GetFromJsonAsync<List<StudentResponse>>("/api/students");
        Assert.Contains(list!, s => s.Id == created.Id && s.Name == "Api Create Test");
    }

    [Fact]
    public async Task Delete_removes_student_then_returns_404_the_second_time()
    {
        var created = await (await _client.PostAsJsonAsync("/api/students", NewStudent("Api Delete Test")))
            .Content.ReadFromJsonAsync<StudentResponse>();

        var first = await _client.DeleteAsync($"/api/students/{created!.Id}");
        var second = await _client.DeleteAsync($"/api/students/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);

        var list = await _client.GetFromJsonAsync<List<StudentResponse>>("/api/students");
        Assert.DoesNotContain(list!, s => s.Id == created.Id);
    }

    [Fact]
    public async Task Post_with_invalid_data_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync("/api/students",
            new { name = "", dateOfBirth = "2000-01-15", phone = "abc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Phone", out _));
    }

    [Fact]
    public async Task Get_unknown_id_returns_404() =>
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync($"/api/students/{Guid.NewGuid()}")).StatusCode);

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
