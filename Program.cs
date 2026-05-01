using Aihrly.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
                       $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")}";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
if (Environment.GetEnvironmentVariable("TESTING") != "true")
{
    builder.Services.AddDbContext<AihrlyDbContext>(options => options.UseNpgsql(connectionString));

    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") ?? "redis://localhost:6379";
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisUrl));
}
else
{
    builder.Services.AddDbContext<AihrlyDbContext>(options => { });
}

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

public partial class Program { }
