using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StudentDbContext>(options =>
options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 42))
)
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
var app = builder.Build();

app.UseCors("AllowReact");


app.MapGet("/students", async (StudentDbContext db) =>
await db.Students.ToListAsync());

app.MapPost("/students", async (Student student, StudentDbContext db) =>
{
    db.Students.Add(student);
    await db.SaveChangesAsync();
    return Results.Created($"/students/{student.Id}", student);
});

app.MapPut("/students/{id}", async (int id, Student updatedStudent, StudentDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student is null) return Results.NotFound();

    student.Name = updatedStudent.Name;
    student.Age = updatedStudent.Age;

    await db.SaveChangesAsync();
    return Results.Ok(student);
});

app.MapDelete("/students/{id}", async (int id, StudentDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student is null) return Results.NotFound();

    db.Students.Remove(student);
    await db.SaveChangesAsync();

    return Results.Ok(student);
});
app.Run();

public class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}
