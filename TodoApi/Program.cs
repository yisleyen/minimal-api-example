using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger settings
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TotoApi V1");
});

// HttGet
app.MapGet("/", () => "Hello Minimal APIs");

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

// Route Handler
string SomeMessage() => "Hello Minimal APIs";
app.MapGet("/hello", SomeMessage);

string Local() => "Minimal APIs";
app.MapGet("/local", Local);

RouteHello routeHello = new RouteHello();
app.MapGet("/instance", routeHello.InstanceMethod);
app.MapGet("/static", RouteHello.StaticMethod);

// HttpContext value bind
app.MapGet("/hello/{name}", (HttpContext ctx) => $"Hello {ctx.Request.RouteValues["name"]}");

// OPTIONS and HEAD method list
app.MapMethods("/options-or-head",
    new[] { "OPTIONS", "HEAD" },
    () => "This is an options or head request");

// HttPost
app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

// HttPut
app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// HttDelete
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

// Console log message
app.Logger.LogInformation("Information message");
app.Logger.LogError("Error message");
app.Logger.LogCritical("Critical message");
app.Logger.LogWarning("Warning message");

// Reading data from appsetting.json
app.Logger.LogInformation($"{app.Configuration["JWT:Key"]}");

//// Multi-port usage
//app.Urls.Add("https://localhost:3000");
//app.Urls.Add("https://localhost:4000");

// Custom port number sample
//app.Run("https://localhost:3000");

// Running from port variable in appsetting.json
//app.Run($"{app.Configuration["JWT:Port"]}");

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

class RouteHello
{
    public string InstanceMethod()
    {
        return "InstanceMethod";
    }

    public static string StaticMethod()
    {
        return "StaticMethod";
    }
}