using practppi.Components;
using practppi.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies; // Нужно для устранения краша

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5143"); 
});

// 👇 ВОЗВРАЩАЕМ ЭТО, чтобы устранить ошибку "No authenticationScheme"
// Но благодаря изменению в Profile.razor, это НЕ вызовет редирект.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/login";
    });
    
builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();