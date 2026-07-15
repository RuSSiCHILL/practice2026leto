using System;
using System.Diagnostics;
using System.IO;

var Sin = (double x) => Math.Sin(x);
double a = -100, b = 100;
double[] steps = { 1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6 };
int maxThreads = 16;
int warmupRuns = 5;
int measureRuns = 20;

double selectedStep = 0;
foreach (var step in steps)
{
    double error = (b - a) / 12.0 * step * step;
    if (error <= 1e-4 && selectedStep == 0)
        selectedStep = step;
}

System.Console.WriteLine($"Замеры времени для шага {selectedStep:E}");
for (int i = 0; i < warmupRuns; i++)
{
    DefiniteIntegral.SolveSingleThread(a, b, Sin, selectedStep);
    DefiniteIntegral.Solve(a, b, Sin, selectedStep, 4);
}
double singleTotal = 0;
for (int i = 0; i < measureRuns; i++)
{
    var sw = Stopwatch.StartNew();
    DefiniteIntegral.SolveSingleThread(a, b, Sin, selectedStep);
    sw.Stop();
    singleTotal += sw.Elapsed.TotalMilliseconds;
}
double singleAvg = singleTotal / measureRuns;
System.Console.WriteLine($"Однопоточное время (среднее из {measureRuns}): {singleAvg:F2} мс");

var results = new List<(int threads, double time)>();
for (int t = 1; t <= maxThreads; t++)
{
    double multiTotal = 0;
    for (int i = 0; i < measureRuns; i++)
    {
        var sw = Stopwatch.StartNew();
        DefiniteIntegral.Solve(a, b, Sin, selectedStep, t);
        sw.Stop();
        multiTotal += sw.Elapsed.TotalMilliseconds;
    }
    double multiAvg = multiTotal / measureRuns;
    results.Add((t, multiAvg));
    double speedup = (singleAvg - multiAvg) / singleAvg * 100;
    System.Console.WriteLine($"Потоков {t,3}: {multiAvg:F2} мс (ускорение: {speedup:F1}%)");
}


var best = results.OrderBy(r => r.time).First();
System.Console.WriteLine($"Оптимальное количество потоков: {best.threads}");
System.Console.WriteLine($"Однопоточное время: {singleAvg:F2} мс");
System.Console.WriteLine($"Многопоточное время: {best.time:F2} мс");
double bestSpeedup = (singleAvg - best.time) / singleAvg * 100;
System.Console.WriteLine($"Ускорение: {bestSpeedup:F1}%");

var plt = new ScottPlot.Plot();
var xs = results.Select(r => (double)r.threads).ToArray();
var ys = results.Select(r => r.time).ToArray();

var scatter = plt.Add.Scatter(xs, ys);
scatter.MarkerSize = 10;
scatter.LineWidth = 2;

plt.Title($"Время вычисления и количество потоков (шаг {selectedStep:E})");
plt.XLabel("Количество потоков");
plt.YLabel("Время (мс)");

plt.Add.Marker(best.threads, best.time, color: ScottPlot.Colors.Red, size: 15, shape: ScottPlot.MarkerShape.OpenCircle);
plt.SavePng("graph.png", 800, 600);

string report = $@"=== Результаты оптимизации ===
Функция: sin(x)
Отрезок: [{a}, {b}]
Точность: 1e-4
Выбранный шаг: {selectedStep:E}
Оптимальное количество потоков: {best.threads}
Однопоточное время: {singleAvg:F2} мс
Многопоточное время: {best.time:F2} мс
Ускорение: {bestSpeedup:F1}%
";
File.WriteAllText("results.txt", report);
Console.WriteLine("Результаты сохранены");

