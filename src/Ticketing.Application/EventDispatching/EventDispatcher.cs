using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider provider, ILogger<EventDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task DispatchAsync(string typeName, string payload, CancellationToken ct)
    {
        var type = EventTypeResolver.Resolve(typeName);
        var @event = JsonSerializer.Deserialize(payload, type);

        if (@event == null)
        {
            _logger.LogError("Failed to deserialize event {EventType}", typeName);
            throw new Exception($"Failed to deserialize event {typeName}");
        }

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        var handlers = _provider.GetServices(handlerType).ToList();

        _logger.LogInformation("Dispatching {EventType} to {HandlerCount} handler(s)",
            typeName, handlers.Count);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method == null)
                throw new Exception("Handler missing HandleAsync method");

            await (Task)method.Invoke(handler, [@event, ct])!;
        }

        _logger.LogInformation("Dispatched {EventType} successfully", typeName);
    }
}