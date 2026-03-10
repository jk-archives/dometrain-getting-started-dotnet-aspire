using Blazored.Modal;
using Frontend;
using Frontend.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddServiceDefaults();

builder.Services.AddBlazoredModal();

// Use "http://" instead of "https+http://" to avoid SSL failures in Kubernetes,
// where inter-pod traffic is plain HTTP and TLS is handled at the ingress level.
// builder.Services.AddHttpClient<BackendHttpClient>(x => x.BaseAddress = new Uri("https+http://api"));
builder.Services.AddHttpClient<BackendHttpClient>(x => x.BaseAddress = new Uri("http://api"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
