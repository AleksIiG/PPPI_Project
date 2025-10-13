using System.Text;
using api.Data;
using api.Interface;
using api.Repesitory;
using api.Services;
using api.Services.Interfaces;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUSerService, UserService>();
builder.Services.AddScoped<IRevorkedTokenService, RevorkedTokenService>();

// JWT config
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET not set");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "my-app";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "my-app-client";
var jwtExpiresMinutes = int.Parse(builder.Configuration["JWT_EXPIRES_MINUTES"] ?? "15");

var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // В production має бути true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // зменшує "плаваючий час"
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection();

// 🔑 Додаємо перевірку автентифікації
app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
// 🔑 Перевірка прав (після auth)
app.UseAuthorization();

app.MapControllers();
app.Run();
