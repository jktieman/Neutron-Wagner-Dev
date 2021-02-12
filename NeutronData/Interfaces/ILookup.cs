namespace NeutronData.Interfaces
{
    public interface ILookup
    {
        int Id { get; set; }
        string Name { get; set; }
        int Sequence { get; set; }
    }
}
