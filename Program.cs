using Aihrly.Data;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
                       $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")}";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AihrlyDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();
app.MapControllers();

app.Run();
