using System;
using Xunit;

public class DefiniteIntegralTests
{
    private static readonly Func<double, double> X = x => x;
    private static readonly Func<double, double> Sin = Math.Sin;

    [Fact]
    public void Solve_Linear_ReturnZero()
    {
        double result = DefiniteIntegral.Solve(-1, 1, X, 1e-4, 2);
        Assert.Equal(0, result, 4);
    }

    [Fact]
    public void Solve_Sin_ReturnsZero()
    {
        double result = DefiniteIntegral.Solve(-1, 1, Sin, 1e-5, 8);
        Assert.Equal(0, result, 4);
    }

    [Fact]
    public void Solve_Linear_Returns12_5()
    {
        double result = DefiniteIntegral.Solve(0, 5, X, 1e-6, 8);
        Assert.Equal(12.5, result, 4);
    }

    [Fact]
    public void Solve_SingleThread()
    {
        double result = DefiniteIntegral.Solve(0, 2, X, 1e-4, 1);
        Assert.Equal(2, result, 3);
    }

    [Fact]
    public void Solve_ZeroThreads()
    {
        Assert.Throws<ArgumentException>(() => DefiniteIntegral.Solve(0, 1, X, 1e-4, 0));
    }

    [Fact]
    public void Solve_MultipleThreads()
    {
        double result1 = DefiniteIntegral.Solve(0, 3, X, 1e-5, 1);
        double result4 = DefiniteIntegral.Solve(0, 3, X, 1e-5, 4);
        Assert.Equal(result1, result4, 4);
    }
}