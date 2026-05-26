using Ticketing.Application;
using Ticketing.Infrastructure;
using Serilog;
using Serilog.Formatting.Json;
using Ticketing.API.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

// Logging configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();
}


app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("MyCorsPolicy");
app.MapControllers();

app.Run();