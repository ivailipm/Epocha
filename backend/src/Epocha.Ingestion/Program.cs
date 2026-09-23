using Epocha.Application;
using Epocha.Infrastructure;
using Epocha.Ingestion;

// Same "composition root" idea as the API's Program.cs: this is where the worker
// process decides which services exist. It reuses exactly the same AddApplication /
// AddInfrastructure registrations as the API, which is the payoff of putting them in
// shared class libraries.
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<IngestionOptions>(builder.Configuration.GetSection(IngestionOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
