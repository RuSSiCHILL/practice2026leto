using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Task19.Tests
{
    public class ServerThreadTests
    {
        [Fact]
        public void ThenHardStop()
        {
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(scheduler);
            var commands = new List<TestCommand>();

            for (int i = 1; i <= 5; i++)
            {
                var cmd = new TestCommand(i, 3);
                commands.Add(cmd);
                server.Enqueue(cmd);
            }

            server.Start();
            
            while (!commands.TrueForAll(c => c.IsCompleted))
            {
                Thread.Sleep(10);
            }

            server.Enqueue(new HardStopCommand(server));
            server.Join(TimeSpan.FromSeconds(3));

            foreach (var cmd in commands)
            {
                Assert.True(cmd.IsCompleted);
                Assert.Equal(3, cmd.Output.Count);
            }
        }

        [Fact]
        public void RunInParallel()
        {
            var scheduler = new RoundRobinScheduler();
            var server = new ServerThread(scheduler);
            var cmd1 = new TestCommand(1, 3);
            var cmd2 = new TestCommand(2, 3);

            server.Enqueue(cmd1);
            server.Enqueue(cmd2);
            server.Start();

            while (!cmd1.IsCompleted || !cmd2.IsCompleted)
                Thread.Sleep(10);

            server.Enqueue(new HardStopCommand(server));
            server.Join(TimeSpan.FromSeconds(3));

            Assert.True(cmd1.Output.Count >= 1);
            Assert.True(cmd2.Output.Count >= 1);
        }

        [Fact]
        public void CompletesAfterMaxCalls()
        {
            var cmd = new TestCommand(1, 3);
            
            Assert.False(cmd.IsCompleted);
            cmd.Execute();
            Assert.False(cmd.IsCompleted);
            cmd.Execute();
            Assert.False(cmd.IsCompleted);
            cmd.Execute();
            Assert.True(cmd.IsCompleted);
        }
    }
}