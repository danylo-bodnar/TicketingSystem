namespace Ticketing.Application.Common.Interfaces
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(string typeName, string payload);
    }
}