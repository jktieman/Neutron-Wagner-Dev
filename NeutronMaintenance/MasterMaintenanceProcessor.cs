using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore;
using System.Windows.Forms;
using NeutronMaintenance.Models;
using AsyncAwaitBestPractices;

namespace NeutronMaintenance
{

    public class MasterMaintenanceProcessor : IMasterMaintenanceProcessor
    {
        private readonly ILocationManager _locationManager;

        private string _maintenanceFileFilter;
        private DirectoryInfo _directoryInfo;
        private readonly IDynamicLogger _logger;

        public MasterMaintenanceProcessor(ILocationManager locationManager)
        {
            _locationManager = locationManager;
            _logger = NeutronCore.Global.Logger.SetupLogger("MasterMaintenanceProcessor");
            Init();
        }

        private void Init()
        {
            try
            {
                _directoryInfo = new DirectoryInfo(LoaderSettings.GetMaintenanceFileDirectory());
                _maintenanceFileFilter = LoaderSettings.GetMaintenanceFileFilter();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"INIT Master Maintenance Files Error. {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
            }
        }

        public void ProcessFiles()
        {
            var files = GetFiles();
            if (files.Any())
            {
                FileProcessor(files);
            }
        }

        private FileInfo[] GetFiles()
        {
            var result = new FileInfo[] { };
            try
            {
                result = _directoryInfo.GetFiles(_maintenanceFileFilter);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Get Master Maintenance Files Error.  {Environment.NewLine} {ex.Message}{Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
            }
            return result;
        }

        public void FileProcessor(FileInfo[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    _logger.LogDetailAsync($"File Name: {file.FullName}").SafeFireAndForget();
                    var allLines = File.ReadAllLines(file.FullName);
                    foreach (var line in allLines)
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        //What kind of line is it>
                        if (line.Contains("RANDOMLOCATION"))
                        {
                            _logger.LogDetailAsync($"RANDOMLOCATION: {line}").SafeFireAndForget();
                            var randomLocation = CreateRandomLocation(line);
                            if (randomLocation != null)
                            {
                                _locationManager.Process(randomLocation);
                            }
                        }
                        else if (line.Contains("RANDOMSKU"))
                        {
                            _logger.LogDetailAsync($"RANDOMSKU").SafeFireAndForget();
                            ProcessRandomSku(line);
                        }
                        else if (line.Contains("OFFCARDEFSKU"))
                        {
                            _logger.LogDetailAsync($"OFFCARDEFSKU").SafeFireAndForget();
                            ProcessOffCarSku(line);
                        }
                        else if (line.Contains("OFFCARDEFSLOT"))
                        {
                            _logger.LogDetailAsync($"OFFCARDEFSLOT").SafeFireAndForget(); 
                            ProcessOffCarLocation(line);
                        }
                        else if (line.Contains("OFFCARRESERVE"))
                        {
                            _logger.LogDetailAsync($"OFFCARRESERVE").SafeFireAndForget();
                            ProcessOffCarInventory(line);
                        }
                        else if (line.Contains("AKADEFINITION"))
                        {
                            _logger.LogDetailAsync($"AKADEFINITION").SafeFireAndForget();
                            ProcessAka(line);
                        }
                        else
                        {
                            _logger.LogDetailAsync($"Last Else").SafeFireAndForget();
                            //if the line doesn't have any of these, it's Inventory
                            // Sku and Location with quantity
                            ProcessInventory(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Error Reading Maintenance Lines. {ex.Message} 
 {ex.InnerException}");
                }
            }
        }

        private void ProcessInventory(string line)
        {
            var inventoryMaintenanceRecords = new List<InventoryLoad>();

            var rec = new InventoryLoad();
            rec.Station = line.Substring(127, 1);
            //rec.StorageType = line.Substring();
            //rec.Item = line.Substring();
            //rec.Description = line.Substring();
            //rec.Quantity = line.Substring();
            //rec.Slot = line.Substring();
            //rec.PrimeBin = line.Substring();
            //rec.Carousel = line.Substring();
            //rec.Bin = line.Substring();
            //rec.Level = line.Substring();
            //rec.Partition = line.Substring();
            //rec.Tag = line.Substring();
            //rec.SizeCode = line.Substring();
            //rec.VelocityCode = line.Substring();
            //rec.HeightCode = line.Substring();
            //rec.UserCode = line.Substring();
            //rec.ReceivedDate = line.Substring();
            //rec.Id = line.Substring();

            inventoryMaintenanceRecords.Add(rec);

        }

        private void ProcessAka(string line)
        {
            var akaRecords = new List<AkaLoad>();
            var akaDefinitionUpdate = new AkaDefinitionUpdate();

            _logger.LogDetailAsync($"AKA: {line}").SafeFireAndForget();
            _logger.LogDetailAsync($"Line Length: {line.Length}").SafeFireAndForget();
            if (line.Length > 49)
            {
                var akaSku = line.Substring(50).Trim();
                _logger.LogDetailAsync($"AKA: {akaSku} Length: {akaSku.Length}").SafeFireAndForget();
                var sku = line.Substring(0, 35).Trim();
                _logger.LogDetailAsync($"AKA: {sku} Length: {sku.Length}").SafeFireAndForget();
                if (akaSku.Length > 0 && sku.Length > 0)
                {
                    var rec = new AkaLoad();
                    rec.AkaSku = akaSku;
                    rec.Item = sku;

                    akaRecords.Add(rec);
                }
            }

            akaDefinitionUpdate.ProcessAkaDefinitions(akaRecords, _logger);

        }

        private void ProcessOffCarInventory(string line)
        {

        }

        private void ProcessOffCarLocation(string line)
        {

        }

        private void ProcessOffCarSku(string line)
        {

        }

        private NovaRandomLocation CreateRandomLocation(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            if (line.Length < 110) return null;

            var rec = new NovaRandomLocation
            {
                Car = line.Substring(22, 2),
                Bin = line.Substring(26, 2),
                Lvl = line.Substring(30, 2),
                Prt = line.Substring(34, 2),
                Velocity = line.Substring(36, 3),
                Size = line.Substring(39, 3),
                Height = line.Substring(42, 3),
                Operation = line.Substring(45, 1),
                SystemNumber = line.Substring(105, 1)
            };
            return rec;
        }

        private void ProcessRandomSku(string line)
        {
            var itemMaintenanceRecords = new List<ItemDefinitionLoad>();
            var itemDefinitionUpdate = new ItemDefinitionUpdate();

            try
            {
                _logger.LogDetailAsync($"RandomSku: {line}").SafeFireAndForget();

                var rec = new ItemDefinitionLoad();
                rec.Station = line.Substring(126, 1);
                rec.Item = line.Substring(0, 35);
                rec.Description = line.Substring(84, 30);
                rec.LocationMax = line.Substring(62, 9);
                rec.LocationMin = line.Substring(73, 9);
                rec.SystemMax = line.Substring(52, 9);
                rec.SystemMin = line.Substring(166, 9);
                rec.SizeCode = line.Substring(186, 3);
                rec.VelocityCode = line.Substring(159, 3);
                rec.HeightCode = string.Empty;   // line.Substring(189,3);
                rec.LocationCode = string.Empty;
                rec.StorageType = GetStorageType(line.Substring(158, 1));
                rec.UnitOfIssue = line.Substring(128, 6);
                rec.Weight = line.Substring(137, 6);
                rec.Scale = GetScale(line.Substring(135, 1));
                rec.Id = string.Empty;
                itemMaintenanceRecords.Add(rec);
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Error Reading ItemDefinitionLoad Lines. {ex.Message} \r\n {ex.InnerException}");
            }

            itemDefinitionUpdate.ProcessItemDefinitions(itemMaintenanceRecords, _logger);

        }

        private string GetScale(string v)
        {
            if (v.ToUpper() == @"Y")
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }

        private string GetStorageType(string v)
        {
            if (v == "R")
            {
                return @"Release";
            }
            else
            {
                return @"Static";
            }
        }

        public void ArchiveFile(FileInfo fileInfo)
        {
            var archiveDir = ($"{_directoryInfo.FullName}Archive\\");
            try
            {
                if (File.Exists(fileInfo.FullName))
                {
                    var archivefile = Path.Combine(archiveDir, fileInfo.Name);
                    if (File.Exists($"{archivefile}"))
                    {
                        File.Delete(archivefile);
                    }

                    fileInfo.MoveTo(archivefile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync("Archive Master Maintenance File Error: " + ex.Message).SafeFireAndForget();
            }
        }

    }

}