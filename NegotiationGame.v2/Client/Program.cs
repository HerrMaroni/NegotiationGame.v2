using FindRazorSourceFile.WebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Services;
using NegotiationGame.v2.Client;
using NegotiationGame.v2.Client.Service;
using NegotiationGame.v2.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// #if DEBUG
// builder.UseFindRazorSourceFile();
// #endif

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("NegotiationGame.v2.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("NegotiationGame.v2.ServerAPI"));

builder.Services.AddApiAuthorization();

builder.Services.AddMudServices();

builder.Services.AddSingleton<GameClient>();
builder.Services.AddSingleton(GameClient.HubConnectionFactory);
builder.Services.AddSingleton<GameData>();


var host = builder.Build();
var hubConnection = await host.Services.GetRequiredService<Task<HubConnection>>();
builder.Services.AddSingleton(hubConnection);

host = builder.Build();

await host.RunAsync();
