using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הזרקת DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

// הוספת שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});



// הוספת שירותי Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDo API", Version = "v1" });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = string.Empty;  // הפניה לדף Swagger בשטח הבסיסי
});


// שימוש במדיניות CORS
app.UseCors("AllowAll");

app.MapGet("/tasks", async (ToDoDbContext db) =>
{
    var tasks = await db.Tasks.ToListAsync();
    return tasks.Any() ? Results.Ok(tasks) : Results.NoContent();
});

app.MapPost("/tasks", async (ToDoDbContext db, TodoApi.Task newTask) =>
{
    db.Tasks.Add(newTask);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

app.MapPut("/tasks/{id}", async (ToDoDbContext db, int id, TodoApi.Task updatedTask) =>
{
    var existingTask = await db.Tasks.FindAsync(id);
    if (existingTask is null)
    {
        return Results.NotFound();
    }

    existingTask.Name = updatedTask.Name;
    existingTask.IsComplete = updatedTask.IsComplete;

    await db.SaveChangesAsync();
    return Results.Ok(existingTask);
});

app.MapDelete("/tasks/{id}", async (ToDoDbContext db, int id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound();
    }

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapGet("/",()=> "API is runing");

app.Run();