
//using Blazored.LocalStorage;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using SmartInventoryManagementSystem.Client;
//using SmartInventoryManagementSystem.Client.Authentication;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//// Simple HttpClient
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7067/") });

//// Blazored local storage
//builder.Services.AddBlazoredLocalStorage();

//// Custom Auth State Provider
//builder.Services.AddScoped<CustomAuthStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
//    provider.GetRequiredService<CustomAuthStateProvider>());

//// Enable authorization
//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<APIService>();

//await builder.Build().RunAsync();

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartInventoryManagementSystem.Client;
using SmartInventoryManagementSystem.Client.Authentication;
using SmartInventoryManagementSystem.Client.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register local storage and auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthMessageHandler>();

builder.Services.AddScoped<APIService>();

// configure an HttpClient that uses the auth handler and points to your API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7067/");
})
.AddHttpMessageHandler<AuthMessageHandler>();

// make the named client available as the default HttpClient injected into components
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

await builder.Build().RunAsync();