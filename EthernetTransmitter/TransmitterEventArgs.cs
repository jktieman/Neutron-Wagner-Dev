using System;

namespace EthernetTransmitter
{
    public class TransmitterEventArgs : EventArgs
    {
        public string FormText;
        public bool Transmitting;
        public string DisplayCSV;
        public string FlashCode;

        public TransmitterEventArgs(string formText, bool transmitting, string displayCsv, string flashCode)
        {
            FormText = formText;
            Transmitting = transmitting;
            DisplayCSV = displayCsv;
            FlashCode = flashCode;
        }
    }
}
