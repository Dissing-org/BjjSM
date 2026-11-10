using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BjjSM.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BjjSM.Client.Services.IMemberApiClient, BjjSM.Client.Services.MemberApiClient>();
builder.Services.AddScoped<BjjSM.Client.Services.ICurriculumApiClient, BjjSM.Client.Services.CurriculumApiClient>();
builder.Services.AddScoped<BjjSM.Client.Services.IAnnualWheelApiClient, BjjSM.Client.Services.AnnualWheelApiClient>();
builder.Services.AddScoped<BjjSM.Client.Services.ISpacedRepetitionApiClient, BjjSM.Client.Services.SpacedRepetitionApiClient>();
builder.Services.AddScoped<BjjSM.Client.Services.ICurriculumMediaApiClient, BjjSM.Client.Services.CurriculumMediaApiClient>();
builder.Services.AddScoped<BjjSM.Client.Services.IFlowchartApiClient, BjjSM.Client.Services.FlowchartApiClient>();

await builder.Build().RunAsync();
