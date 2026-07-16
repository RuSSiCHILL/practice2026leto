using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Task18;

double MeasureTime(int totalSteps, int stepsPerExecute, int commandCount)
{
    var scheduler = new RoundRobinScheduler();
    var server = new ServerThread(scheduler);

    for (int i = 0; i < commandCount; i++)
        server.Enqueue(new MeasuredCommand(totalSteps, stepsPerExecute));

    var sw = Stopwatch.StartNew();
    server.Start();

    // Ждём пока все команды завершатся
    while (scheduler.HasCommand())
        Thread.Sleep(1);

    // Останавливаем через SoftStop изнутри
    server.Enqueue(new SoftStopCommand(server));
    server.Join(TimeSpan.FromSeconds(10));
    sw.Stop();

    return sw.Elapsed.TotalMilliseconds;
}

var quantumSizes = new[] { 1, 2, 5, 10, 20, 50, 100 };
int totalSteps = 200;
int commandCount = 2;
int runs = 1;

var results = new List<(int quantum, double time)>();

Console.WriteLine("Квант | Время (мс)");
Console.WriteLine("------+-----------");

foreach (var quantum in quantumSizes)
{
    double totalTime = 0;
    for (int i = 0; i < runs; i++)
        totalTime += MeasureTime(totalSteps, quantum, commandCount);
    double avgTime = totalTime / runs;
    results.Add((quantum, avgTime));
    Console.WriteLine($"{quantum,5} | {avgTime,10:F2}");
}

// Сохраняем текстовый отчёт
var lines = new List<string>
{
    "=== Результаты исследования планировщика (задание 18) ===",
    $"Общее количество шагов на команду: {totalSteps}",
    $"Количество команд: {commandCount}",
    $"Количество прогонов: {runs}",
    "",
    "Размер кванта | Время (мс)",
    "--------------+------------"
};

foreach (var (quantum, time) in results)
    lines.Add($"{quantum,13} | {time,10:F2}");

var best = results.OrderBy(r => r.time).First();
var worst = results.OrderByDescending(r => r.time).First();

lines.Add("");
lines.Add($"Оптимальный квант: {best.quantum} ({best.time:F2} мс)");
lines.Add($"Худший квант: {worst.quantum} ({worst.time:F2} мс)");
lines.Add($"Разница: {worst.time - best.time:F2} мс");

File.WriteAllLines("optimization_results.txt", lines);
Console.WriteLine("\nОтчёт сохранён в optimization_results.txt");

// График
try
{
    var plt = new ScottPlot.Plot();
    var xs = results.Select(r => (double)r.quantum).ToArray();
    var ys = results.Select(r => r.time).ToArray();
    
    var scatter = plt.Add.Scatter(xs, ys);
    scatter.MarkerSize = 10;
    scatter.Color = ScottPlot.Color.FromHex("#1f77b4");
    
    plt.Title("Зависимость времени выполнения от размера кванта");
    plt.XLabel("Размер кванта (шагов за Execute)");
    plt.YLabel("Время (мс)");
    
    // Отмечаем оптимум красным
    var opt = plt.Add.Scatter(new[] { (double)best.quantum }, new[] { best.time });
    opt.MarkerSize = 15;
    opt.Color = ScottPlot.Color.FromHex("#d62728");
    
    plt.SavePng("quantum.png", 800, 600);
    Console.WriteLine("График сохранён в quantum.png");
}
catch (Exception ex)
{
    Console.WriteLine($"График не создан (нет ScottPlot): {ex.Message}");
}

// Команда для замера
public class MeasuredCommand : ICommand
{
    private readonly int _totalSteps;
    private int _currentStep;
    private readonly int _stepsPerExecute;
    public bool IsCompleted { get; private set; }

    public MeasuredCommand(int totalSteps, int stepsPerExecute)
    {
        _totalSteps = totalSteps;
        _stepsPerExecute = stepsPerExecute;
    }

    public void Execute()
    {
        if (!IsCompleted)
        {
            for (int i = 0; i < _stepsPerExecute && _currentStep < _totalSteps; i++)
            {
                _currentStep++;
                Thread.SpinWait(1000);
            }
            if (_currentStep >= _totalSteps)
                IsCompleted = true;
        }
    }
}