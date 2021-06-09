using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Hart_DisplayControllers;


namespace Neutron.Forms
{
    public partial class FrmRemstar : Form
    {
        private readonly Hart_DisplayController _hDisplay;
        private const int ComPort = 1;
        private string _cError = "";
        private int _address = 0;
        private Hart_SHI _myShi;

        private List<Hart_SHI> _bob = new List<Hart_SHI>();

        public FrmRemstar()
        {
            InitializeComponent();
            var logFile = @"c:\Neutron\Logs\Hart\Bob.log";

            _hDisplay = new Hart_DisplayController(Hart_DisplayController.Controller_Type_Remstar_BPI_SHI(), logFile);
        }

        private void btn_Cancel_Click(object sender, System.EventArgs e)
        {
            _cError = "";
            _hDisplay.Close_Controller(ref _cError);
            Close();
        }

        private void btn_Clear_Click(object sender, System.EventArgs e)
        {

            // Passed all tests, user entered "Address"

            //Hart_SHI mySHI = new Hart_SHI(_address);
            _cError = "";

            if (_hDisplay.Clear(_myShi, ref _cError))
            {
                // If the function call returned true, everything is good.
                // // If mySHI is a list, clearing everything in the list was successful.
                MessageBox.Show($"Clear display at address {_address} was Successful. {Environment.NewLine} {_cError}");
            }
            else
            {
                // Something went wrong
                if (_cError.Trim().Length > 0)
                    // If cError contains text, it was a systemic error and it never even tried clearing the individual displays
                    MessageBox.Show("Clear display at address Failed. {Environment.NewLine} {cError}");
                //else
                //{
                //    // if mySHI is a list, you should check every member as some may have cleared, some not.
                //    if (!_bob.SHI_Successful)
                //        MessageBox.Show("Clear display at address " + _address.ToString() + " Failed." + "\n\n" + mySHI.SHI_ErrorMessage);
                //    else
                //        MessageBox.Show("Clear display at address " + _address.ToString() + " Failed.");

            }
        }
        
        private void btn_Show_Click(object sender, System.EventArgs e)
        {

            // User entered "Address", txt_Partition.Text, txt_Quantity.Text
            // Also rad_Left_Off,  rad_Left_ON,  rad_Left_Blink
            // Also rad_Right_Off, rad_Right_ON, rad_Right_Blink
            _address = int.Parse(txt_Address.Text);

            if (_address == 5)
            {
                // This is a BPI
                var myBpi = new Hart_BPI(_address, 0, txt_Partition.Text + txt_Quantity.Text);

                if (rad_Left_Off.Checked) { myBpi.BPI_Beacon_Off(); }
                if (rad_Left_On.Checked) { myBpi.BPI_Beacon_On(); }
                if (rad_Left_Blink.Checked) { myBpi.BPI_Beacon_Blink(); }


                // Passed all tests
                _cError = "";

                if (_hDisplay.Show(myBpi, ref _cError))
                {
                    // If the function call returned true, everything is good.
                    // If myBPI is a list, showing everything in the list was successful.
                    MessageBox.Show("Illuminate display at address " + _address.ToString() + " was Successful.");
                }
                else
                {
                    // Something went wrong
                    if (_cError.Trim().Length > 0)
                        // If cError contains text, it was a systemic error and it never even tried illuminating the individual displays
                        MessageBox.Show("Illuminate display at address " + _address.ToString() + " Failed." + "\n\n" + _cError);
                    else
                    {
                        // if myBPI is a list, you should check every member as some may have been illuminated, some not.
                        if (!myBpi.BPI_Successful)
                            MessageBox.Show("Illuminate display at address " + _address.ToString() + " Failed." + "\n\n" + myBpi.BPI_ErrorMessage);
                        else
                            MessageBox.Show("Clear display at address " + _address.ToString() + " Failed.");
                    }
                }
            }
            else
            {
                for (var i = 0; i < 6; i++)
                {
                    _address += i;
                    var myShi = new Hart_SHI(_address, 0, 0, txt_Partition.Text, txt_Quantity.Text);
                    if (rad_Left_Off.Checked) { myShi.SHI_BeaconLeft_Off(); }
                    if (rad_Left_On.Checked) { myShi.SHI_BeaconLeft_On(); }
                    if (rad_Left_Blink.Checked) { myShi.SHI_BeaconLeft_Blink(); }

                    if (rad_Right_Off.Checked) { myShi.SHI_BeaconRight_Off(); }
                    if (rad_Right_On.Checked) { myShi.SHI_BeaconRight_On(); }
                    if (rad_Right_Blink.Checked) { myShi.SHI_BeaconRight_Blink(); }

                    _bob.Add(myShi);
                }


                // Hart_SHI mySHIb = new Hart_SHI(Address, 0, 0, txt_Partition.Text, txt_Quantity.Text);



                //if (rad_Left_Off.Checked) { mySHIb.SHI_BeaconLeft_Off(); }
                //if (rad_Left_On.Checked) { mySHIb.SHI_BeaconLeft_On(); }
                //if (rad_Left_Blink.Checked) { mySHIb.SHI_BeaconLeft_Blink(); }

                //if (rad_Right_Off.Checked) { mySHIb.SHI_BeaconRight_Off(); }
                //if (rad_Right_On.Checked) { mySHIb.SHI_BeaconRight_On(); }
                //if (rad_Right_Blink.Checked) { mySHIb.SHI_BeaconRight_Blink(); }


                //// "bob" is the name of the list, just because...

                //List<Hart_SHI> bob = new List<Hart_SHI>();
                //bob.Add(mySHI);
                //bob.Add(mySHIb);


                // Passed all tests
                _cError = "";

                // if (hDisplay.Show( mySHI, ref cError))                                                        // for a single display
                if (_hDisplay.Show(_bob, ref _cError))                                                              // for a list of displays
                {
                    // If the function call returned true, everything is good.
                    //        MessageBox.Show("Illuminate display at address " + Address.ToString() + " was Successful.");    // message for a single display
                    // If mySHI is a list, showing everything in the list was successful.
                    MessageBox.Show("Illumination at all displays was Successful.");                                // message for a list of displays
                }
                else
                {
                    // Something went wrong
                    if (_cError.Trim().Length > 0)
                    {
                        // If cError contains text, it was a systemic error and it never even tried illuminating the individual displays
                        //            MessageBox.Show("Illuminate display at address " + Address.ToString() + " Failed." + "\n\n" + cError);
                        // message for a single display
                        MessageBox.Show("Illumination Failed." + "\n\n" + _cError);
                        // message for a list of displays
                    }
                    else
                    {
                        // if you only passed a single struct...
                        //           if (!mySHI.SHI_Successful)
                        //                MessageBox.Show("Illuminate display at address " + Address.ToString() + " Failed." + "\n\n" + mySHI.SHI_ErrorMessage);

                        // if mySHI is a list, you should check every member as some may have been illuminated, some not.
                        foreach (var shi in _bob)
                        {
                            if (!shi.SHI_Successful)
                                MessageBox.Show("Illuminate display at address " + shi.SHI_Address.ToString() + " Failed." + "\n\n" + shi.SHI_ErrorMessage);
                        }
                    }
                }
            }
        }

        private void btn_Init_Click(object sender, System.EventArgs e)
        {
            var cError = "";

            if (_hDisplay.Init_Controller(ComPort, false, 2, ref cError))
            {
                MessageBox.Show("Initialization Successful");
            }
            else
            {
                MessageBox.Show("Initialization Failed" + "\n\n" + cError);
            }
        }

        private void btn_ShowOne_Click(object sender, EventArgs e)
        {
            _address = int.Parse(txt_Address.Text);

            _myShi = new Hart_SHI(_address, 0, 0, txt_Partition.Text, txt_Quantity.Text);
            if (rad_Left_Off.Checked) { _myShi.SHI_BeaconLeft_Off(); }
            if (rad_Left_On.Checked) { _myShi.SHI_BeaconLeft_On(); }
            if (rad_Left_Blink.Checked) { _myShi.SHI_BeaconLeft_Blink(); }

            if (rad_Right_Off.Checked) { _myShi.SHI_BeaconRight_Off(); }
            if (rad_Right_On.Checked) { _myShi.SHI_BeaconRight_On(); }
            if (rad_Right_Blink.Checked) { _myShi.SHI_BeaconRight_Blink(); }

            // Passed all tests
            _cError = "";

            // if (hDisplay.Show( mySHI, ref cError)) for a single display
            if (_hDisplay.Show(_myShi, ref _cError))
            {
                //If the function call returned true, everything is good.
                MessageBox.Show("Illuminate display at address " + _address.ToString() + " was Successful.");
                // message for a single display
            }
            else
            {
                // Something went wrong
                if (_cError.Trim().Length > 0)
                {
                    // If cError contains text, it was a systemic error and it never even tried illuminating the individual displays
                    MessageBox.Show("Illuminate display at address " + _address.ToString() + " Failed." + "\n\n" + _cError);
                    // message for a single display
                }
                else
                {
                    // if you only passed a single struct...
                    if (!_myShi.SHI_Successful)
                        MessageBox.Show("Illuminate display at address " + _address.ToString() + " Failed." + "\n\n" + _myShi.SHI_ErrorMessage);
                }
            }
        }
    }
}
