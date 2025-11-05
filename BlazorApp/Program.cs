using BlazorApp.Components; // <-- ИСПРАВЛЕНО (было BlazorApp)


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Api", client =>
{
    // Указываем базовый адрес твоего бэкенда (из Swagger)
    client.BaseAddress = new Uri("http://localhost:5143"); 
});



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // <-- Мы оставляем это. Весь сайт будет интерактивным.

app.Run();