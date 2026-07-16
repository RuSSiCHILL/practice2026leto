using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Task19;

double MeasureTime(int totalCalls, int stepsPerExecute, int commandCount)
{
    var scheduler = new RoundRobinScheduler();
    var server = new ServerThread(scheduler);
    var commands = new List<TestCommand>();

    for (int i = 0; i < commandCount; i++)
    {
        var cmd = new TestCommand(i + 1, totalCalls);
        commands.Add(cmd);
        server.Enqueue(cmd);
    }

    var sw = Stopwatch.StartNew();
    server.Start();

    while (!commands.TrueForAll(c => c.IsCompleted))
        Thread.Sleep(1);

    server.Enqueue(new HardStopCommand(server));
    server.Join(TimeSpan.FromSeconds(10));
    sw.Stop();

    return sw.Elapsed.TotalMilliseconds;
}

var totalCallsList = new[] { 1, 3, 5, 10, 20, 50, 100 };
int commandCount = 5;
int runs = 3;

var results = new List<(int calls, double time)>();

Console.WriteLine("Вызовов | Время (мс)");
Console.WriteLine("--------+-----------");

foreach (var calls in totalCallsList)
{
    double totalTime = 0;
    for (int i = 0; i < runs; i++)
        totalTime += MeasureTime(calls, 1, commandCount);
    double avgTime = totalTime / runs;
    results.Add((calls, avgTime));
    Console.WriteLine($"{calls,7} | {avgTime,10:F2}");
}

var lines = new List<string>
{
    "=== Результаты исследования (задание 19) ===",
    $"Количество команд: {commandCount}",
    $"Количество прогонов: {runs}",
    "",
    "Вызовов на команду | Время (мс)",
    "--------------------+------------"
};

foreach (var (calls, time) in results)
    lines.Add($"{calls,19} | {time,10:F2}");

File.WriteAllLines("optimization_results.txt", lines);
Console.WriteLine("\nОтчёт сохранён в optimization_results.txt");

try
{
    var plt = new ScottPlot.Plot();
    var xs = results.Select(r => (double)r.calls).ToArray();
    var ys = results.Select(r => r.time).ToArray();
    
    plt.Add.Scatter(xs, ys);
    plt.Title("Время выполнения vs количество вызовов Execute");
    plt.XLabel("Количество вызовов на команду");
    plt.YLabel("Время (мс)");
    plt.SavePng("quantum.png", 800, 600);
    Console.WriteLine("График сохранён в quantum.png");
}
catch (Exception ex)
{
    Console.WriteLine($"График не создан: {ex.Message}");
}