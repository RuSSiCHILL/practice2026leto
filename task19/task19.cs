using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Task19
{
    public interface ICommand
    {
        void Execute();
        bool IsCompleted { get; }
    }

    public interface IScheduler
    {
        bool HasCommand();
        ICommand? Select();
        void Add(ICommand cmd);
    }

    public class RoundRobinScheduler : IScheduler
    {
        private readonly List<ICommand> _commands = new();
        private int _currentIndex = 0;
        private readonly object _lock = new();

        public bool HasCommand()
        {
            lock (_lock)
            {
                return _commands.Count > 0;
            }
        }

        public ICommand? Select()
        {
            lock (_lock)
            {
                _commands.RemoveAll(c => c.IsCompleted);
                
                if (_commands.Count == 0)
                    return null;

                if (_currentIndex >= _commands.Count)
                    _currentIndex = 0;

                var command = _commands[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _commands.Count;
                return command;
            }
        }

        public void Add(ICommand cmd)
        {
            lock (_lock)
            {
                _commands.Add(cmd);
            }
        }
    }

    public class ServerThread : IDisposable
    {
        private readonly BlockingCollection<ICommand> _queue = new();
        private readonly IScheduler _scheduler;
        private Thread? _thread;
        private volatile bool _isRunning;
        private volatile bool _hardStop;
        private volatile bool _softStop;
        private bool _disposed;

        public Thread? ProcessingThread => _thread;
        public bool IsRunning => _isRunning;

        public ServerThread(IScheduler scheduler)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void Start()
        {
            if (_isRunning)
                throw new InvalidOperationException("Поток уже запущен");
            
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServerThread));

            _isRunning = true;
            _hardStop = false;
            _softStop = false;

            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "ServerThread"
            };
            _thread.Start();
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
                while (!_hardStop)
                {
                    if (_queue.TryTake(out ICommand? newCommand, TimeSpan.FromMilliseconds(100)))
                    {
                        _scheduler.Add(newCommand);
                        continue;
                    }

                    if (_scheduler.HasCommand())
                    {
                        var command = _scheduler.Select();
                        if (command != null)
                        {
                            ExecuteCommand(command);
                            continue;
                        }
                    }

                    if (_softStop && !_scheduler.HasCommand() && _queue.Count == 0)
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

        private void ExecuteCommand(ICommand command)
        {
            try
            {
                command.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в команде: {ex.Message}");
            }

            if (!command.IsCompleted)
            {
                _scheduler.Add(command);
            }
        }

        public void HardStop()
        {
            if (Thread.CurrentThread != _thread)
                throw new InvalidOperationException("HardStop должен вызываться из того же потока");
            _hardStop = true;
        }

        public void SoftStop()
        {
            if (Thread.CurrentThread != _thread)
                throw new InvalidOperationException("SoftStop должен вызываться из того же потока");
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
                _hardStop = true;
                _queue.CompleteAdding();
                Join(TimeSpan.FromSeconds(5));
                _queue.Dispose();
            }
        }
    }

    public class TestCommand : ICommand
    {
        private readonly int _id;
        private int _counter;
        private readonly int _maxCalls;
        public bool IsCompleted { get; private set; }
        public List<string> Output { get; } = new();

        public TestCommand(int id, int maxCalls = 3)
        {
            _id = id;
            _maxCalls = maxCalls;
        }

        public void Execute()
        {
            _counter++;
            var message = $"Поток {_id} вызов {_counter}";
            Output.Add(message);
            
            if (_counter >= _maxCalls)
                IsCompleted = true;
        }
    }

    public class HardStopCommand : ICommand
    {
        private readonly ServerThread _server;
        public bool IsCompleted => true;

        public HardStopCommand(ServerThread server)
        {
            _server = server;
        }

        public void Execute()
        {
            _server.HardStop();
        }
    }
}