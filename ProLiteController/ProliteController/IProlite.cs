namespace ProliteController
{
    public interface IProlite
    {
        string TurnOn(int level, int part, int quantity);
        string Clear();
        string TurnOnBlindCycle(int level, int part);
        string TurnOnHot();
    }
}