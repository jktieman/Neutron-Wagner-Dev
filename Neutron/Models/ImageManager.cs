using System.Drawing;
using System.IO;
using AlliedLogger;
using Neutron.Interfaces;
using NeutronCore;
using Logger = NeutronCore.Global.Logger;

namespace Neutron.Models
{
    public class ImageManager : IImageManager
    {
        private readonly IDynamicLogger _logger;

        public ImageManager()
        {
            _logger = Logger.SetupLogger(@"ImageManager");
        }
        public string GetImageFile(string item = "")
        {
         _ = _logger.LogDetailAsync($"GetImageFile: {item}");

            var localItem = item.ToLower().Trim();
            string imageFile = null;
            var imagesDirectory = LoaderSettings.GetImagesDirectory();
            if (!string.IsNullOrEmpty(item))
            {
                var file = Path.Combine(imagesDirectory, $"{localItem}.jpg");
                if (File.Exists(file))
                {
                    imageFile = file;
                }
                else
                {
                    file = Path.Combine(imagesDirectory, $"no-image.png");
                    if (File.Exists(file)) imageFile = file;
                }
            }
            else
            {
                var file = Path.Combine(imagesDirectory, $"no-image.png");
                if (File.Exists(file)) imageFile = file;
            }
         _ = _logger.LogDetailAsync($"Return ImageFile: {imageFile}");
            return imageFile;
        }

    }
}
