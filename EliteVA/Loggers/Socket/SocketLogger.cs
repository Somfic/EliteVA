using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WatsonWebsocket;

namespace EliteVA.Loggers.Socket;

public class SocketLogger : ILogger
{
    private readonly WatsonWsServer _server;
    private readonly ConcurrentQueue<Message> _backlog = new();
    private readonly Timer _countdown;

    public SocketLogger(WatsonWsServer server)
    {
        _server = server;
        _countdown = new Timer(LogsHaveBeenAccumulated, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void LogsHaveBeenAccumulated(object state)
    {
        try
        {
            if (!_server.ListClients().Any())
                return;

            List<Message> messages = new();

            while (_backlog.TryDequeue(out var message))
            {
                messages.Add(message);
            }

            var json = JsonConvert.SerializeObject(messages);

            foreach (var client in _server.ListClients().Where(x => _server.IsClientConnected(x.Guid)))
            {
                _server.SendAsync(client.Guid, json).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        try
        {
            if (!_server.ListClients().Any())
                return;
            
            var message = new Message(logLevel, eventId, formatter(state, exception), exception);
            _backlog.Enqueue(message);

            _countdown.Change(500, Timeout.Infinite);
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    private readonly struct Message
    {
        public Message(LogLevel logLevel, EventId eventId, string state, Exception? exception)
        {
            LogLevel = logLevel.ToString().ToUpper();
            EventId = eventId;
            State = state;
            Exception = exception;
        }
        
        public DateTime Timestamp => DateTime.Now;
        public string LogLevel { get; }
        public EventId EventId { get; }
        public string State { get; }
        public Exception? Exception { get; }
    }
}