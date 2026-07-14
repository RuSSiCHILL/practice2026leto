using System;
using System.Threading;

public class DefiniteIntegral
{
    public static double Solve(double a, double b, Func<double, double> function, double step, int threadsNumber)
    {
        if (threadsNumber < 1)
            throw new ArgumentException("Число потоков должно быть больше 0", nameof(threadsNumber));
        double totalResult = 0;
        double segmentLength = (b - a) / threadsNumber;
        using var barrier = new Barrier(threadsNumber);
        var threads = new Thread[threadsNumber];
        for (int i = 0; i < threadsNumber; i++)
        {
            int threadIndex = i;
            double localA = a + threadIndex * segmentLength;
            double localB = localA + segmentLength;
            threads[i] = new Thread(() =>
            {
                double localResult = CalculateIntegral(localA, localB, function, step);
                double original, sum;
                do
                {
                    original = totalResult;
                    sum = original + localResult;
                }
                while (Interlocked.CompareExchange(ref totalResult, sum, original) != original);
                barrier.SignalAndWait();
            });

            threads[i].Start();
        }
        foreach (var thread in threads)
            thread.Join();
        return totalResult;
    }
    private static double CalculateIntegral(double a, double b, Func<double, double> function, double step)
    {
        double result = 0;
        int stepsCount = (int)((b - a) / step);
        for (int i = 0; i < stepsCount; i++)
        {
            double x1 = a + i * step;
            double x2 = x1 + step;
            result += (function(x1) + function(x2)) / 2 * step;
        }
        return result;
    }
}