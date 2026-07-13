using Xunit;

public class CalculatorGeneratorTests
{
    private readonly ICalculator _calculator;

    public CalculatorGeneratorTests()
    {
        _calculator = new CalculatorGenerator().Generate();
    }

    [Fact]
    public void Add_ReturnsSum()
    {
        Assert.Equal(5, _calculator.Add(2, 3));
    }

    [Fact]
    public void Minus_ReturnsDifference()
    {
        Assert.Equal(3, _calculator.Minus(7, 4));
    }

    [Fact]
    public void Mul_ReturnsProduct()
    {
        Assert.Equal(12, _calculator.Mul(3, 4));
    }

    [Fact]
    public void Div_ReturnsQuotient()
    {
        Assert.Equal(2, _calculator.Div(6, 3));
    }

    [Fact]
    public void Generate_ReturnsCalculator()
    {
        Assert.NotNull(_calculator);
        Assert.IsAssignableFrom<ICalculator>(_calculator);
    }
}
