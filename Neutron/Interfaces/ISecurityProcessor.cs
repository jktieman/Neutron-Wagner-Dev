namespace Neutron.Interfaces
{
    public interface ISecurityProcessor
    {
        bool[] SecurityProfile { get; set; }
        void ReprocessSecuritySet(string id);
    }
}
