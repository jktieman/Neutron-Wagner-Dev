using System;
using System.Windows.Forms;
using NeutronData.Models;

namespace Neutron.Controllers
{
    public class Ipti_SHI
    {
        private readonly TowerLevelInfo _towerLevelInfo;
        private int _lBeacon;
        private readonly string _part;
        private int _rBeacon;
        private readonly string _text;
        private const string DisplayCommand = "39";
        private const string Space = " ";
        private const string Dash = "-";
        private readonly string _leftArrow = Convert.ToChar(19).ToString();
        private readonly string _rightArrow = Convert.ToChar(18).ToString();

        //code to clear is the same with no text

        public Ipti_SHI(TowerLevelInfo towerLevelInfo, int lBeacon, int rBeacon, string part, string text)
        {
            _towerLevelInfo = towerLevelInfo;
            _lBeacon = lBeacon;
            _rBeacon = rBeacon;
            _part = part.Trim().PadLeft(2, ' ');
            _text = text.Trim().PadLeft(6, ' ');
        }

        public string TurnOn()
        {
            var result = "";
            try
            {
                switch (_towerLevelInfo.ArrowDirection)
                {
                    case "Left":
                        result = _towerLevelInfo.BayId.PadLeft(2, '0') + DisplayCommand + _towerLevelInfo.Display.PadLeft(2, '0') + _leftArrow + Dash + Space + _part + _text;
                        break;
                    case "Right":
                        result = _towerLevelInfo.BayId.PadLeft(2, '0') + DisplayCommand + _towerLevelInfo.Display.PadLeft(2, '0') + _part + Space + _text + Space + Dash + _rightArrow;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to turn On display.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            return result;
        }

        public string TurnOff()
        {
            var result = "";
            try
            {
                result = _towerLevelInfo.BayId.PadLeft(2, '0') + "39" + _towerLevelInfo.Display.PadLeft(2, '0');
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to turn Off display.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            return result;
        }

        public string BayId => _towerLevelInfo.BayId.PadLeft(2, '0');

        public string DisplayId => _towerLevelInfo.Display.PadLeft(2, '0');
    }
}