using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace IPTI.Models
{
    public class OrderControlModule
    {
        private readonly string _ocId;
        private const string _turnOn = "27";
        private const string _turnOff = "14";
        private string _ledState = "2";

        public string LedState
        {
            get => _ledState;
            set => _ledState = value;
        }

        public OrderControlModule(string ocId, string buttonColorId)
        {
            _ledState = buttonColorId;
            _ocId = ocId.PadLeft(2, '0');
        }

        public string TurnOn(string text)
        {
            return _turnOn + _ocId + _ledState + "0" + text;
        }

        public string Clear()
        {
            return _turnOff + "OC" + _ocId;
        }
       
    }
}
