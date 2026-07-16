using System;
using System.Collections.Concurrent;
using System.Threading;


public interface ICommand
{
    void Execute();
}

public class HardStopCommand : ICommand
{
    private readonly ServerThread _server;
    
    public HardStopCommand(ServerThread server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }
    
    public void Execute()
    {
        if (Thread.CurrentThread != _server.ProcessingThread)
            throw new InvalidOperationException(
                "HardStop должен вызываться из того же потока, который выполняет команды");
        _server.RequestHardStop();
    }
}

public class SoftStopCommand : ICommand
{
    private readonly ServerThread _server;
    
    public SoftStopCommand(ServerThread server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    public void Execute()
    {
        if (Thread.CurrentThread != _server.ProcessingThread)
            throw new InvalidOperationException(
                "SoftStop должен вызываться из того же потока, который выполняет команды");
        
        _server.RequestSoftStop();
    }
}

public class ServerThread : IDisposable
{
    private readonly BlockingCollection<ICommand> _queue = new();
    private Thread? _thread;
    private volatile bool _hardStop;
    private volatile bool _softStop;
    private volatile bool _isRunning;
    private readonly object _lock = new();
    private bool _disposed;

    public Thread? ProcessingThread => _thread;
    public bool IsRunning => _isRunning;

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning)
                throw new InvalidOperationException("Поток уже запущен");
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServerThread));
            _isRunning = true;
            _isStarted = true;
            _hardStop = false;
            _softStop = false;

            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "ServerThread"
            };
            _thread.Start();
        }
    }

    public void Enqueue(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        if (_disposed)
            throw new ObjectDisposedException(nameof(ServerThread));
        _queue.Add(command);
    }
    private void Run()
    {
        try
        {
            foreach (var command in _queue.GetConsumingEnumerable())
            {
                if (_hardStop)
                    break;
                try
                {
                    command.Execute();
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Handle(command, ex);
                }
                if (_softStop && _queue.Count == 0)
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Критическая ошибка в ServerThread: {ex}");
        }
        finally
        {
            _isRunning = false;
            _thread = null;
        }
    }
    internal void RequestHardStop()
    {
        _hardStop = true;
    }
    internal void RequestSoftStop()
    {
        _softStop = true;
    }
    public void Join()
    {
        _thread?.Join();
    }
    public bool Join(TimeSpan timeout)
    {
        return _thread?.Join(timeout) ?? true;
    }
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _queue.CompleteAdding();
            Join(TimeSpan.FromSeconds(5));
            _queue.Dispose();
        }
    }
}
public static class ExceptionHandler
{
    public static void Handle(ICommand command, Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Ошибка в команде {command.GetType().Name}: {exception.Message}");
    }
}
