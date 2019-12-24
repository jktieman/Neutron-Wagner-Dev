using AlliedLogger;
using Hart_DisplayControllers;
using JsonManager;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Controllers
{
    public class DisplayController : IDisplayController
    {
        private readonly GenericRepository<SerialConfiguration> _repoSerial = new GenericRepository<SerialConfiguration>(new NeutronDb());

        public Hart_DisplayController HartDisplayController;

        private string cError = string.Empty;
        private DynamicLogger _logger;
        private string logFileDir = string.Empty;
        private readonly StationView _station;
        private readonly List<Hart_BLI> bliList = new List<Hart_BLI>();
        private readonly List<Hart_SHI> shiList = new List<Hart_SHI>();
        private bool ready;
        private int lBeacon = 0;
        private int rBeacon = 0;
        private readonly bool bliEnabled = true;
        private readonly bool shiEnabled = true;
        private readonly NeutronVariables neutronVariables;


        List<Hart_BLI> blisOn = new List<Hart_BLI>();
        List<Hart_SHI> shisOn = new List<Hart_SHI>();

        public event EventHandler<IptiController.MySerialDataReceivedEventArgs> MySerialDataReceived;

        public DisplayController(IJsonData jsonData, StationView station)
        {
            _station = station;
            neutronVariables = jsonData.LoadFile<NeutronVariables>();
            bliEnabled = neutronVariables.BliEnabled;
            shiEnabled = neutronVariables.ShiEnabled;

            CreateLog();

            //if (station.StationNumber == 4 || station.StationNumber == 5)
            //{
            //    if (bliEnabled)
            //    {
            //        FillBliListStations4_5();
            //    }
            //}

            //else
            //{
            if (bliEnabled)
            {
                FillBliList();
            }
            if (shiEnabled)
            {
                FileShiList();
            }
            // }

            string hartLog = ($"{logFileDir}Hart");
            HartDisplayController = new Hart_DisplayController(Hart_DisplayController.Controller_Type_Remstar_BPI_SHI(), hartLog);
            Task.Run(() => _logger.Log(msg: @"HartDisplayController has been created: "));
            if (HartDisplayControllerInit())
            {
                ready = true;
            }
            else
            {
                Task.Run(() => _logger.Log(@"HartDisplayController failed Initialization"));
                ready = false;
            }
        }

        private void CreateLog()
        {

            logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = string.Format(format: @"Display Controller_{0}", arg0: _station.StationNumber.ToString());
            string logActivity = LoaderSettings.EnableLogging;
            Task.Run(() => _logger = new DynamicLogger(logFileDir, folderName, logActivity));
        }

        public void CloseController()
        {
            try
            {
                if (HartDisplayController != null)
                {
                    HartDisplayController.Close_Controller(ref cError);
                    Task.Run(() => _logger.Log($"Close Display Controller Closed {cError}"));
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Close Display Controller Exception: {cError} {Environment.NewLine}{ex.Message} {Environment.NewLine} {ex.InnerException} "));
            }
        }


        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }

        private bool HartDisplayControllerInit()
        {
            bool result = false;
            string serialConfigurationName = ($"Com4_Displays");
            SerialConfiguration serialConfiguration = _repoSerial.FindBy(r => r.Name == serialConfigurationName).FirstOrDefault();

            if (serialConfiguration != null)
            {
                Task.Run(() => _logger.Log($"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()}"));
                Task.Run(() => _logger.Log($"Serial Port Number: {serialConfiguration.PortNumber.ToString()}"));
                if (HartDisplayController.Init_Controller(serialConfiguration.PortNumber, serialConfiguration.SimulationMode, serialConfiguration.LogLevel, ref cError))
                {
                    Task.Run(() => _logger.Log("Initialization Requested"));
                    result = true;
                }
                else
                {
                    Task.Run(() => _logger.Log("Problem requesting initialization. " + cError));
                }
            }
            else
            {
                Task.Run(() => _logger.Log("SerialConfiguration is null "));
            }
            return result;
        }

        //public void ShowAll()
        //{
        //    if (station.StationNumber == 4 || station.StationNumber == 5)
        //    {
        //        ShowAllBli();
        //    }

        //    else
        //    {
        //        ShowAllBli();
        //        ShowAllShi();
        //    }
        //}

        //public void ClearAll()
        //{
        //    if (station.StationNumber == 4 || station.StationNumber == 5)
        //    {
        //        ClearAllBli();
        //    }

        //    else
        //    {
        //        ClearAllBli();
        //        ClearAllShi();
        //    }
        //}

        //public void ShowAllBli()
        //{

        //    if (bliEnabled)
        //    {
        //        Task.Run(() => logger.Log("BLI Show All Displays."));
        //        if (!HartDisplayController.Show(bliList, ref cError))
        //        {
        //            Task.Run(() => logger.Log("BLI Show All Display Error."));
        //        }
        //    }
        //}

        public void ClearAllBli()
        {
            if (bliEnabled)
            {

                var blisOnDelete = new List<Hart_BLI>();

                foreach (var bli in blisOn)
                {
                    if (blisOn.Contains(bli))
                    {
                        Task.Run(() => _logger.Log($"BLI Clear All Displays.  {bli.BLI_Address}"));
                        Thread.Sleep(10);
                        if (!HartDisplayController.Clear(bli, ref cError))
                        {
                            Task.Run(() => _logger.Log($"BLI Clear All Display Error.  {bli.BLI_Address} \r\n  {cError}"));
                        }
                        blisOnDelete.Add(bli);
                    }
                }
                foreach (var item in blisOnDelete)
                {
                    blisOn.Remove(item);
                }
            }
        }

        //public void ShowAllShi()
        //{
        //    if (shiEnabled)
        //    {
        //        Task.Run(() => logger.Log("SHI Show All Displays."));
        //        if (!HartDisplayController.Show(shiList, ref cError))
        //        {
        //            Task.Run(() => logger.Log("SHI Show All Display Error."));
        //        }
        //    }
        //}

        public void ClearAllShi()
        {
            if (shiEnabled)
            {
                Task.Run(() => _logger.Log("SHI Clear All Displays."));
                var shisOnDelete = new List<Hart_SHI>();
                foreach (var shi in shisOn)
                {
                    if (shisOn.Contains(shi))
                    {
                        Task.Run(() => _logger.Log($"SHI Clear All Displays.  {shi.SHI_Address}"));
                        Thread.Sleep(10);
                        if (!HartDisplayController.Clear(shi, ref cError))
                        {
                            Task.Run(() => _logger.Log($"SHI Clear All Display Error.  {shi.SHI_Address} \r\n  {cError}"));
                        }
                        shisOnDelete.Add(shi);
                    }
                }
                foreach (var item in shisOnDelete)
                {
                    shisOn.Remove(item);
                }
            }
        }

        //public void ShowListBli(List<Hart_BLI> blis)
        //{
        //    if (bliEnabled)
        //    {
        //        Task.Run(() => logger.Log("BLI Show List Displays."));
        //        if (!HartDisplayController.Show(blis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("BLI Show List Display Error."));
        //        }
        //    }
        //}

        //public void ShowListShi(List<Hart_SHI> shis)
        //{
        //    if (shiEnabled)
        //    {
        //        Task.Run(() => logger.Log("SHI Show List Displays."));
        //        if (!HartDisplayController.Show(shis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("SHI Show List Display Error."));
        //        }
        //    }
        //}

        //public void ClearListBli(List<Hart_BLI> blis)
        //{
        //    if (bliEnabled)
        //    {
        //        Task.Run(() => logger.Log("BLI Clear List Displays."));
        //        if (!HartDisplayController.Clear(blis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("BLI Clear List Display Error."));
        //        }
        //    }
        //}

        //public void ClearListShi(List<Hart_SHI> shis)
        //{
        //    if (shiEnabled)
        //    {
        //        Task.Run(() => logger.Log("SHI Clear List Displays."));
        //        if (!HartDisplayController.Clear(shis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("SHI Clear List Display Error."));
        //        }
        //    }
        //}

        public void ShowBli(int address, int beacon, string text)
        {
            if (bliEnabled)
            {
                Task.Run(() => _logger.Log($"BLI Address: {address}"));
                var bli = new Hart_BLI(address, 2, text);
                if (!blisOn.Contains(bli))
                {
                    blisOn.Add(bli);
                }
                Task.Run(() => _logger.Log($"BLI On: {bli.BLI_Address}"));
                Thread.Sleep(10);
                if (!HartDisplayController.Show(bli, ref cError))
                {
                    Task.Run(() => _logger.Log($"BLI Show Single Display Error.  { bli.BLI_Address}\r\n {cError}"));
                }
            }
        }

        public void ShowBli(Hart_BLI bli)
        {
            if (bliEnabled)
            {
                if (!blisOn.Contains(bli))
                {
                    blisOn.Add(bli);
                }
                Task.Run(() => _logger.Log($"Hart BLI Address: {bli.BLI_Address}"));
                Thread.Sleep(10);
                if (!HartDisplayController.Show(bli, ref cError))
                {
                    Task.Run(() => _logger.Log($"Hart BLI Show Single Display Error.  { bli.BLI_Address}\r\n {cError}"));
                }
            }
        }

        public void ShowShi(int device, int bin, int level, string part, string text)
        {
            if (shiEnabled)
            {
                Task.Run(() => _logger.Log($"ShowShi -- Device: {device}  Bin: {bin}  Level: {level}  Part: {part}  Text: {text}"));
                int address = GetAddress(device, level);
                var shi = new Hart_SHI(address, lBeacon, rBeacon, part, text);
                if (!shisOn.Contains(shi))
                {
                    shisOn.Add(shi);
                }
                Task.Run(() => _logger.Log($"SHI Show: {shi.SHI_Address}"));
                Thread.Sleep(10);
                if (!HartDisplayController.Show(shi, ref cError))
                {
                    Task.Run(() => _logger.Log($"SHI Show Single Display Error.  {shi.SHI_Address}\r\n {cError}"));
                }

            }
        }

        public void ShowShi(Hart_SHI shi)
        {
            if (shiEnabled)
            {
                if (!shisOn.Contains(shi))
                {
                    shisOn.Add(shi);
                }

                Task.Run(() => _logger.Log($"Hart SHI Show: {shi.SHI_Address}"));
                Thread.Sleep(10);
                if (!HartDisplayController.Show(shi, ref cError))
                {
                    Task.Run(() => _logger.Log($"Hart SHI Show Single Display Error 2.  {shi.SHI_Address}\r\n {cError}"));
                }
            }
        }

        public void ClearBli(Hart_BLI bli)
        {
            if (bliEnabled)
            {
                if (blisOn.Contains(bli))
                {
                    Task.Run(() => _logger.Log($"BLI Clear Single Display. {bli.BLI_Address}"));
                    if (!HartDisplayController.Clear(bli, ref cError))
                    {
                        Task.Run(() => _logger.Log($"BLI Clear Single Display Error.  {bli.BLI_Address}\r\n {cError}"));
                    }
                    blisOn.Remove(bli);
                }
            }
        }

        public void ClearShi(Hart_SHI shi)
        {
            if (shiEnabled)
            {
                if (shisOn.Contains(shi))
                {
                    Task.Run(() => _logger.Log($"SHI Clear Single Display.  {shi.SHI_Address}"));
                    Thread.Sleep(10);
                    if (!HartDisplayController.Clear(shi, ref cError))
                    {
                        Task.Run(() => _logger.Log($"SHI Clear Single Display Error.   {shi.SHI_Address} \r\n { cError}"));
                    }
                    shisOn.Remove(shi);
                }
            }
        }

        public int GetInitStatus()
        {
            Task.Run(() => _logger.Log("Get Init Status"));
            int stat = InitStatus();
            Task.Run(() => _logger.Log($"Get Init Status - Return: {stat}"));
            return stat;
        }

        public void ShowOc(int address, int beacon, string text)
        {
            //throw new NotImplementedException();
        }

        public void ClearOc(int address)
        {
            //throw new NotImplementedException();
        }

        private int InitStatus()
        {
            // Note that the sequesnce of the following assignments is critical. Success must be first. Others follow in any sequence.
            bool success = HartDisplayController.Init_Success;
            int initCode = HartDisplayController.LastStatus_Code;
            string initMsg = HartDisplayController.LastStatus_Message;

            Task.Run(() => _logger.Log($"HartDisplayController InitStatus: Success: {success} initCode: {initCode} initMsg: {initMsg}"));
            if (success)
            {
                // life is good, you can drive the device
                Task.Run(() => _logger.Log($"HartDisplayController InitStatus: success is {success}"));
                if (initCode == 0)
                {
                    // life is good, no warning messages
                    Task.Run(() => _logger.Log($"HartDisplayController Initialization is complete and was successful initCode is {initCode}"));
                }
                else
                {
                    // You need to report the warning to the operator or to a log that is monitored frequently
                    Task.Run(() => _logger.Log($"HartDisplayController Warning - Initialization was successful but there is a warning." + Environment.NewLine +
                    "Please provide the following information to your IT support." + Environment.NewLine +
                    "Code is: " + initCode.ToString() + Environment.NewLine +
                    "Message is: " + initMsg));
                }
            }
            else
            {
                // Darn, cannot drive the device at this time
                if (HartDisplayController.Init_PercentageComplete == 0)
                {
                    Task.Run(() => _logger.Log($"Not Initialized.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else if (HartDisplayController.Init_PercentageComplete < 100)
                {
                    Task.Run(() => _logger.Log($"Initialization is in progress.  Init {HartDisplayController.Init_PercentageComplete.ToString()}% complete..."));
                    Task.Run(() => _logger.Log($"Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else
                {
                    Task.Run(() => _logger.Log($"Initialization was unsuccessful.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
            }
            return initCode;
        }


        private void FileShiList()
        {
            int lBeacon = 2;
            int rBeacon = 2;
            for (int i = 1; i <= 8; i++)
            {
                int address = ($"10{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                shiList.Add(shi);
            }
            for (int i = 1; i <= 8; i++)
            {
                int address = ($"20{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                shiList.Add(shi);
            }
            var sb = new StringBuilder();
            foreach (var item in shiList)
            {
                sb.AppendLine($"SHI - {item.SHI_Address}");
            }
            Task.Run(() => _logger.Log($"SHI Listing\n\r {sb.ToString()}"));
        }

        private void FillBliList()
        {
            for (int i = 1; i <= 16; i++)
            {
                var bli = new Hart_BLI(i, 2, @"------");
                bliList.Add(bli);
            }
            var sb = new StringBuilder();
            foreach (var item in bliList)
            {
                sb.AppendLine($"BLI - {item.BLI_Address}");
            }
            Task.Run(() => _logger.Log($"BLI Listing\n\r {sb.ToString()}"));
        }

        //private void FillBliListStations4_5()
        //{
        //    for (int i = 1; i <= 4; i++)
        //    {
        //        var bli = new Hart_BLI(i, 2, @"------");
        //        bliList.Add(bli);
        //    }
        //    var sb = new StringBuilder();
        //    foreach (var item in bliList)
        //    {
        //        sb.AppendLine($"BLI - {item.BLI_Address}");
        //    }
        //    Task.Run(() => _logger.Log($"BLI Listing\n\r {sb.ToString()}"));
        //}

        private int GetAddress(int device, int level)
        {
            var address = string.Empty;

            switch (_station.StationNumber)
            {
                case 1:
                    {
                        switch (device)
                        {
                            case 1:
                                lBeacon = 2;
                                rBeacon = 0;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 2:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 3:
                                lBeacon = 2;
                                rBeacon = 0;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 4:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                        }
                        break;
                    }
                case 2:  //Standard Setup
                    {
                        switch (device)
                        {
                            case 1:
                                lBeacon = 2;
                                rBeacon = 0;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 2:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 3:
                                lBeacon = 2;
                                rBeacon = 0;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 4:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                        }

                        break;
                    }
                case 3: //Unique Number 3
                    {
                        switch (device)
                        {
                            case 1:
                                lBeacon = 2;
                                rBeacon = 0;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 2:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"1{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 3:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                            case 4:
                                lBeacon = 0;
                                rBeacon = 2;
                                address = ($"2{level.ToString().PadLeft(2, paddingChar: '0')}");
                                break;
                        }
                        break;
                    }
            }

            Task.Run(() => _logger.Log($"Get Address Returned: {address}"));

            return address.ParseInt();

        }

        public void ShowBli(Ipti_BLI bli)
        {
            throw new NotImplementedException();
        }

        public void ClearBli(Ipti_BLI bli)
        {
            throw new NotImplementedException();
        }

        class BliOn
        {
            public int Id { get; set; }
            public Hart_BLI Bli { get; set; }
        }

        class ShiOn
        {
            public int Id { get; set; }
            public Hart_BLI Bli { get; set; }
        }
    }
}
