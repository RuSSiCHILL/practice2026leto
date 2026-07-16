using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Task18.Tests
{
    public class LongRunningCommand : ICommand
    {
        private readonly int _totalSteps;
        private int _currentStep;
        public bool IsCompleted { get; private set; }
        public int StepsExecuted => _currentStep;
        public LongRunningCommand(int totalSteps = 5)
        {
            _totalSteps = totalSteps;
        }
        public void Execute()
        {
            if (!IsCompleted)
            {
                _currentStep++;
                Thread.Sleep(10);
                if (_currentStep >= _totalSteps)
                    IsCompleted = true;
            }
        }
    }

    public class SimpleCommand : ICommand
    {
        private readonly Action _action;
        public bool IsCompleted { get; private set; }

        public SimpleCommand(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action();
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

    public class SoftStopCommand : ICommand
    {
        private readonly ServerThread _server;
        public bool IsCompleted => true;

        public SoftStopCommand(ServerThread server)
        {
            _server = server;
        }

        public void Execute()
        {
            _server.SoftStop();
        }
    }

    public class ServerThreadTests
    {
        [Fact]
        public void LongRunningCommand_ExecutesInMultipleSteps()
        {
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(scheduler);
            var command = new LongRunningCommand(5);
            server.Enqueue(command);
            server.Enqueue(new SoftStopCommand(server));
            server.Start();
            server.Join(TimeSpan.FromSeconds(3));
            Assert.True(command.IsCompleted);
            Assert.Equal(5, command.StepsExecuted);
        }

        [Fact]
        public void MultipleLongCommands_RunInParallel()
        {
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(scheduler);
            var command1 = new LongRunningCommand(3);
            var command2 = new LongRunningCommand(3);
            server.Enqueue(command1);
            server.Enqueue(command2);
            server.Enqueue(new SoftStopCommand(server));
            server.Start();
            server.Join(TimeSpan.FromSeconds(3));
            Assert.True(command1.IsCompleted);
            Assert.True(command2.IsCompleted);
        }

        [Fact]
        public void LongCommand_DoesNotBlockSimpleCommands()
        {
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(scheduler);
            var result = new List<int>();
            var longCommand = new LongRunningCommand(10);
            var simpleCommand = new SimpleCommand(() => result.Add(1));
            server.Enqueue(longCommand);
            server.Enqueue(simpleCommand);
            server.Enqueue(new HardStopCommand(server));
            server.Start();
            server.Join(TimeSpan.FromSeconds(3));
            Assert.Contains(1, result);
        }

        [Fact]
        public void Scheduler_RoundRobinWorks()
        {
            var scheduler = new RoundRobinScheduler();
            var cmd1 = new LongRunningCommand(3);
            var cmd2 = new LongRunningCommand(3);
            scheduler.Add(cmd1);
            scheduler.Add(cmd2);
            var first = scheduler.Select();
            var second = scheduler.Select();
            var third = scheduler.Select();
            Assert.Equal(cmd1, first);
            Assert.Equal(cmd2, second);
            Assert.Equal(cmd1, third);
        }
    }
}