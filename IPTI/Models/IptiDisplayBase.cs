namespace IPTI.Models
{
    public abstract class IptiDisplayBase
    {
        protected string TurnOnCommand;
        protected string TurnOffCommand;

        protected string AStateColor;
        protected string OnTime;
        protected string BStateColor;
        protected string OffTime;
        protected string ButtonControl = "0";
        protected string Arrows = "0";
        protected string LedState = "4";

        public abstract string TurnOn(string quantity);
        public abstract string TurnOnEnd();
        public abstract string TurnOff();
    }
}
