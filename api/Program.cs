using api.Data;
using api.Interface;
using api.Repesitory;

using api.Services;
using api.Services.Interfaces;

using dotenv.net;

DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { ".env" }));

var builder = WebApplication.CreateBuilder(args);

// Додаємо всі джерела конфігурації
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // appsettings
    .AddEnvironmentVariables(); // ENV та .env

// Реєстрація сервісів
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();

builder.Services.AddScoped<IExerciseService, ExerciseService>();


builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
