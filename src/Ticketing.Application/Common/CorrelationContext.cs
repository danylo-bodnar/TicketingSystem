namespace Ticketing.Application.Common
{
    public static class CorrelationContext
    {
        private static readonly AsyncLocal<string?> _correlationId = new();

        public static string? Current => _correlationId.Value;
        public static void Set(string id) => _correlationId.Value = id;
    }
}