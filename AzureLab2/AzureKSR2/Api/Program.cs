using Api.Workers;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<QueueProcessorWorker>();

var app = builder.Build();
app.MapControllers();
app.Run();