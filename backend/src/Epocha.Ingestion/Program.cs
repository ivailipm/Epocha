using Epocha.Application;
using Epocha.Infrastructure;
using Epocha.Ingestion;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<IngestionOptions>(builder.Configuration.GetSection(IngestionOptions.SectionName));
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
