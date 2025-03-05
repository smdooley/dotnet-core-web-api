using Microsoft.EntityFrameworkCore;
using ToDoMinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add the database context to the DI container
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Enable the API Explorer and generate OpenAPI document
builder.Services.AddEndpointsApiExplorer();

// Adds the Swagger OpenAPI document generator to the application services and configures it to provide more information about the API
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoMinimalApi";
    config.Title = "Todo Minimal API";
    config.Version = "v1";
});

var app = builder.Build();

// Enable the Swagger middleware for serving the generated JSON document and the Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Todo Minimal API";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

/*
Adds var todoItems = app.MapGroup("/todoitems"); to set up the group using the URL prefix /todoitems.
Changes all the app.Map<HttpVerb> methods to todoItems.Map<HttpVerb>.
Removes the URL prefix /todoitems from the Map<HttpVerb> method calls.
*/

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", async (TodoDb db) =>
    await db.Todos.ToListAsync());

todoItems.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

todoItems.MapGet("/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

todoItems.MapDelete("/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();