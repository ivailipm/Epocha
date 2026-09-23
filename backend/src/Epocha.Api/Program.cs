using Epocha.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Wires up EpochaDbContext (and, later, anything else Infrastructure owns) using
// builder.Configuration, which is where appsettings.json / appsettings.Development.json
// end up merged at this point.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
