namespace Neutron.Interfaces

{
    public interface INeutronRootDirectory
    {
        string RootDirectory { get; set; }
        string SetRootDirectory();
    }
}