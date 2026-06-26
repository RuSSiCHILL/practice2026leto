public interface ISpaceship
{
    void MoveForward();      // Движение вперед
    void Rotate(int angle);  // Поворот на угол (градусы)
    void Fire();             // Выстрел ракетой
    int Speed { get; }       // Скорость корабля
    int FirePower { get; }   // Мощность выстрела
}
public class Cruiser: ISpaceship
{
    public void MoveForward()=>Distance+=Speed;
    public void Rotate(int corner)=>Сorner=(Сorner+corner)%360;
    public void Fire()
    {
        if (Ammunition>0){
            Ammunition--;
        }   
    }
    public int Speed => 50;
    public int FirePower =>100;


    public int Distance {get; set; }
    public int Сorner {get; set; }
    public int Ammunition {get; set; }=10;
}

public class Fighter: ISpaceship
{
    public void MoveForward()=>Distance+=Speed;
    public void Rotate(int corner)=>Сorner=(Сorner+corner)%360;
    public void Fire()
    {
        if (Ammunition>0){
            Ammunition--;
        }   
    }
    public int Speed => 100;
    public int FirePower =>50;


    public int Distance {get; set; }
    public int Сorner {get; set; }
    public int Ammunition {get; set; }=20;
}
