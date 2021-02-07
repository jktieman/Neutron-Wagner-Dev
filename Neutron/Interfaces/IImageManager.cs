using System.Threading.Tasks;

namespace Neutron.Interfaces
{
    public interface IImageManager
    {
        string GetImageFile(string item = @"");
    }
}