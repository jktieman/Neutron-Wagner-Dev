namespace NeutronCore.Global
{

    public class NeutronVariables
    {
        public bool CreateStoreOrderWithRts = false;
        public bool ShuttleEnabled = true;
        public bool SendAllPicksToHost = false;
        public bool UsePrimeBin = true;
        public string PickMethod = "FIFO";
        public bool UseLAC = false;
        public bool UseMenuSecurity = false;
        public bool UseReturnToStock = false;
        public string DeviceDriver = "None";
        public bool SimulationMode = false;
        public int LogLevel = 2;
        public string SlotNameType = "Default";
        public bool AutoLogOff = false;
        public bool CheckForUsedItem = false;
        public bool RunLoaderOnStartup = false;
        public bool RunUploadOnStartup = false;
        public bool DisplaysEnabled = true;
        public bool EnableDocumentPrinter = false;
        public bool EnableLabelPrinter = false;
        public bool PinLoginOnly = true;
        public int PickBatchSize = 8;
        public int StoreBatchSize = 8;
        public int PickBatchRows = 1;
        public int StoreBatchRows = 1;
        public bool BliEnabled = false;
        public bool ShiEnabled = false;
        public bool ParkPositionAfterBatch = false;
        public bool UsePr1Processor = false;
        public bool UsePr1StyleInputProcessor = false;
        public bool UsePr1StyleOutputProcessor = false;
        public string FieldDelimiter = @"|";
        public bool AutoEnlargeImage { get; set; }
        public bool IptiDisplays { get; set; }
        public bool LoadRackOrders { get; set; }
        public bool SerialPicking { get; set; }
        public bool PrintPreview { get; set; }
        public bool UpdateItemDefinitionDescription { get; set; }
        public bool PrintPackingListStart { get; set; }
        public bool PrintPackingListEnd { get; set; }
        public bool PrintPackingListManual { get; set; }
        public int LoaderDelay { get; set; }
        public int UploadDelay { get; set; }
        public string ActionCodes { get; set; }
        public bool UseCostCenter { get; set; }
        public bool UseImages { get; set; }
        public string DefaultLanguage { get; set; }
        public int DeviceFlashRate { get; set; }
        public int WorkstationId { get; set; }
        public int DefaultStorageTypeId { get; set; }
        public bool UseAutoCompress { get; set; }
        public int CompressDays { get; set; }
        public double RunCompressInterval { get; set; }
        public bool SpecialBackOrder { get; set; }
        public bool EnableEmailNotification { get; set; }
        public int LoaderStation { get; set; }
        public bool RfidEnabledInventory { get; set; }
        public bool RfidEnabledPicking { get; set; }
        public int BliController { get; set; }
    }
}
