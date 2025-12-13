using DotNet8Sample;
using DotNet8Sample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var builder = Host.CreateApplicationBuilder(args);

// Bind Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

// IHostedService registration
builder.Services.AddHostedService<BasicHostedService>();

// BackgroundService registration (PeriodicTimer Pattern)
//builder.Services.AddHostedService<BackgroundTimerService>();

// Resilient BackgroundService registration (Polly + PeriodicTimer Pattern)
//builder.Services.AddHostedService<ResilientWorkerService>();

var app = builder.Build();
await app.RunAsync();