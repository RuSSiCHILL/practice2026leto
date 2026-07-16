using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Task17.Tests
{
    public class TestCommand : ICommand
    {
        private readonly Action _action;
        public bool WasExecuted { get; private set; }

        public TestCommand(Action action)
        {
            _action = action?? throw new ArgumentNullException(nameof(action));
        }

        public void Execute()
        {
            WasExecuted = true;
            _action();
        }
    }

    public class ServerThreadTests
    {
        [Fact]
        public void HardStop_StopsImmediately()
        {
            var server = new ServerThread();
            var result = new List<int>();
            
            server.Enqueue(new TestCommand(() => 
            {
                result.Add(1);
                Thread.Sleep(50);
            }));
            server.Enqueue(new HardStopCommand(server));
            server.Enqueue(new TestCommand(() => result.Add(2)));
            
            server.Start();
            server.Join(TimeSpan.FromSeconds(2));
            
            Assert.Single(result);
            Assert.Equal(1, result[0]);
            Assert.False(server.IsRunning);
        }

        [Fact]
        public void SoftStop_ExecutesAllPendingCommands()
        {
            var server = new ServerThread();
            var result = new List<int>();
            
            server.Enqueue(new TestCommand(() => result.Add(1)));
            server.Enqueue(new TestCommand(() => result.Add(2)));
            server.Enqueue(new SoftStopCommand(server));
            server.Enqueue(new TestCommand(() => result.Add(3)));
            
            server.Start();
            server.Join(TimeSpan.FromSeconds(2));
            
            Assert.Equal(3, result.Count);
            Assert.False(server.IsRunning);
        }

        [Fact]
        public void HardStop_FromOtherThread_Throws()
        {
            var server = new ServerThread();
            var readyEvent = new ManualResetEventSlim(false);
            
            server.Enqueue(new TestCommand(() => 
            {
                readyEvent.Set();
                Thread.Sleep(200);
            }));
            
            server.Start();
            readyEvent.Wait();
            var hardStop = new HardStopCommand(server);
            Assert.Throws<InvalidOperationException>(() => hardStop.Execute());
            server.Enqueue(new HardStopCommand(server));
            server.Join(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void SoftStop_FromOtherThread_Throws()
        {
            var server = new ServerThread();
            var readyEvent = new ManualResetEventSlim(false);
            
            server.Enqueue(new TestCommand(() => 
            {
                readyEvent.Set();
                Thread.Sleep(200);
            }));
            server.Start();
            readyEvent.Wait();
            var softStop = new SoftStopCommand(server);
            Assert.Throws<InvalidOperationException>(() => softStop.Execute());
            server.Enqueue(new HardStopCommand(server));
            server.Join(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Exception_DoesNotCrashThread()
        {
            var server = new ServerThread();
            var result = new List<int>();
            server.Enqueue(new TestCommand(() => throw new InvalidOperationException("test")));
            server.Enqueue(new TestCommand(() => result.Add(1)));
            server.Enqueue(new HardStopCommand(server));
            server.Start();
            server.Join(TimeSpan.FromSeconds(2));
            Assert.Single(result);
            Assert.Equal(1, result[0]);
        }

        [Fact]
        public void EmptyQueue_ThreadStaysAlive()
        {
            var server = new ServerThread();
            server.Start();
            Thread.Sleep(200);
            Assert.True(server.IsRunning, "Поток должен оставаться живым при пустой очереди");
            var result = new List<int>();
            server.Enqueue(new TestCommand(() => result.Add(1)));
            server.Enqueue(new HardStopCommand(server));
            
            server.Join(TimeSpan.FromSeconds(2));
            
            Assert.Single(result);
            Assert.Equal(1, result[0]);
        }

        [Fact]
        public void MultipleStarts_ThrowsException()
        {
            var server = new ServerThread();
            server.Start();
            
            Assert.Throws<InvalidOperationException>(() => server.Start());
            
            server.Enqueue(new HardStopCommand(server));
            server.Join(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Commands_ExecuteInOrder()
        {
            var server = new ServerThread();
            var order = new List<int>();
            
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                server.Enqueue(new TestCommand(() => order.Add(index)));
            }
            server.Enqueue(new SoftStopCommand(server));
            
            server.Start();
            server.Join(TimeSpan.FromSeconds(2));
            
            Assert.Equal(10, order.Count);
            for (int i = 0; i < 10; i++)
                Assert.Equal(i, order[i]);
        }
    }
}