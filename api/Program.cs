using api.Data;
using dotenv.net;

DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] {".env"}));
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Реєстрація MongoDbService
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
