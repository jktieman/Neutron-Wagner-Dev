using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neutron.Interfaces;
using NeutronCore;

namespace Neutron.Models
{
    public class ImageManager : IImageManager
    {
        public string GetImageFile(string item = @"")
        {
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

            return imageFile;
        }

    }
}
