using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api.Services.Interfaces;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
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

        JwtSecurityToken jwt;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            jwt = handler.ReadJwtToken(tokenStr);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Invalid JWT format: {Err}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid token format." });
            return;
        }

        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(jti))
        {
            // 🔹 Створюємо scope для scoped сервісу
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

        await _next(context);
    }
}
