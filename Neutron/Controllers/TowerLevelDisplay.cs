namespace Neutron.Controllers
{
    public partial class IptiController
    {
        public class TowerLevelDisplay
        {
            public int Device { get; set; }
            public int Level { get; set; }
            public string BayId { get; set; }
            public string Display { get; set; }
            public string ArrowDirection { get; set; }
        }
    }
}