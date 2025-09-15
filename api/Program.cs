using api.Data;
using dotenv.net;





// Завантаження змінних середовища з .env файлу
DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { ".env" }));
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDbService>();






var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

