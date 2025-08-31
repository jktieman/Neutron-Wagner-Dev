using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronCore.Models
{
    public interface IDialogService
    {
        Task<bool> ShowAsync(string title, string message, string okButtonText, string cancelButtonText);
        bool Show2(string title, string message, string okButtonText, string cancelButtonText);
        bool TowerError(string title, string message, string okButtonText, string cancelButtonText);
    }
}
