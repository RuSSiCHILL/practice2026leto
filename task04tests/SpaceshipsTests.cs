global using Xunit;


public class SpaceshipTests
{
    [Fact]
    public void Cruiser_ShouldHaveCorrectStats()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(50, cruiser.Speed);
        Assert.Equal(100, cruiser.FirePower);
    }

    [Fact]
    public void Fighter_ShouldBeFasterThanCruiser()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > cruiser.Speed);
    }

    [Fact]
    public void Fighter_ShouldHaveCorrectStats()
    {
        ISpaceship fighter = new Fighter();
        Assert.Equal(100, fighter.Speed);
        Assert.Equal(50, fighter.FirePower);
    }

    [Fact]
    public void MoveForward_ShouldIncreaseDistance()
    {
        var fighter = new Fighter();
        var initial = fighter.Distance;
        fighter.MoveForward();
        Assert.Equal(initial+fighter.Speed, fighter.Distance);
    }

    [Fact]
    public void Rotate_ShouldChangeCorner(){
        var cruiser=new Cruiser();
        cruiser.Rotate(45);
        Assert.Equal(45, cruiser.Сorner);
    }

    [Fact]
    public void Fire_ShouldDecreaseAmmunition(){
        var fighter=new Fighter();
        var initial = fighter.Ammunition;
        fighter.Fire();
        Assert.Equal(initial-1, fighter.Ammunition);
    }
}