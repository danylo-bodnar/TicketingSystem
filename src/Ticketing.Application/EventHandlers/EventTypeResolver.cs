using Ticketing.Domain.Events;

namespace Ticketing.Application.Events
{
    public static class EventTypeResolver
    {
        private static readonly Dictionary<string, Type> _cache = new();

        public static Type Resolve(string typeName)
        {
            if (_cache.TryGetValue(typeName, out var cached))
                return cached;

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    typeof(IEvent).IsAssignableFrom(t) &&
                    t.Name == typeName);

            if (type == null)
                throw new InvalidOperationException($"Unknown event type: {typeName}");

            _cache[typeName] = type;

            return type;
        }
    }
}