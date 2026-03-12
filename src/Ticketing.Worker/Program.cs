using Ticketing.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ReservationExpirationWorker>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<ReservationExpirationWorker>();

var host = builder.Build();
host.Run();
