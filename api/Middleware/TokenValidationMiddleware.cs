using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger, IServiceProvider serviceProvider, IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        var tokenStr = authHeader.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]!));

        try
        {
            // 🔹 Валідація токена згідно з параметрами з TokenService
            handler.ValidateToken(tokenStr, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, // Перевіряє exp і nbf
                ClockSkew = TimeSpan.Zero, // Без додаткової похибки 5 хв
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["JWT_ISSUER"],
                ValidAudience = _config["JWT_AUDIENCE"],
                IssuerSigningKey = key
            }, out SecurityToken validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;

            // 🔹 Отримуємо JTI (ідентифікатор токена)
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                using var scope = _serviceProvider.CreateScope();
                var revokedService = scope.ServiceProvider.GetRequiredService<IRevorkedTokenService>();

                var isRevoked = await revokedService.IsRevokedAsync(jti);
                if (isRevoked)
                {
                    _logger.LogInformation("Revoked token used: jti={Jti}, user={User}", jti, sub);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked." });
                    return;
                }
            }

            // Якщо все ок
            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogInformation("Expired token used.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Token has expired." });
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Invalid token: {Err}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating token.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Internal server error." });
        }
    }
}
