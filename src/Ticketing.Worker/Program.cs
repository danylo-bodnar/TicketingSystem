using Ticketing.Application;
using Ticketing.Infrastructure;
using Ticketing.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ReservationExpirationWorker>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHostedService<ReservationExpirationWorker>();
builder.Services.AddHostedService<OutboxProcessor>();

var host = builder.Build();
host.Run();
