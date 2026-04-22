using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;

    public EventDispatcher(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task DispatchAsync(string typeName, string payload)
    {
        var type = EventTypeResolver.Resolve(typeName);

        var @event = JsonSerializer.Deserialize(payload, type);

        if (@event == null)
            throw new Exception($"Failed to deserialize event {typeName}");

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);

        var handlers = _provider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");

            if (method == null)
                throw new Exception("Handler missing HandleAsync method");

            await (Task)method.Invoke(handler, [@event])!;
        }
    }
}