using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using AlliedFileSystemWatcher;
using AlliedLogger;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using JsonManager;
using NeutronCore.Models;
using System.Windows.Forms;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{

    public class MasterMaintenanceProcessor
    {
        readonly string masterFileFilter; 
        readonly DirectoryInfo masterPath; 
        readonly DynamicLogger logger;

        public MasterMaintenanceProcessor()
        {
            try
            {
                masterPath = new DirectoryInfo(LoaderSettings.GetMaintenanceFileDirectory());
                masterFileFilter = LoaderSettings.GetMaintenanceFileFilter();

                string logFileDir = LoaderSettings.GetLogFileDirectory();
                string folderName = @"Master Maintenance";
                string logActivity = LoaderSettings.EnableLogging;
                logger = new DynamicLogger(logFileDir, folderName, logActivity);
            }
            catch (Exception ex)
            {
                logger.Log($"INIT Master Maintenance Files Error.  \r\n {ex.Message} \r\n {ex.InnerException.Message} \r\n  {ex.InnerException.InnerException.Message}");
            }
        }

        public void ProcessMasterMaintenanceFiles()
        {
            FileInfo[] files = GetFiles();
            if (files.Count() > 0)
            {
                FileProcessor(files);
            }
        }

        private FileInfo[] GetFiles()
        {
            var result = new FileInfo[] { };
            try
            {
                result = masterPath.GetFiles(masterFileFilter);
            }
            catch (Exception ex)
            {
                logger.Log($"Get Master Maintenance Files Error.  \r\n {ex.Message} \r\n {ex.InnerException.Message} \r\n  {ex.InnerException.InnerException.Message}");
            }
            return result;
        }

        public void FileProcessor(FileInfo[] files)
        {
            foreach (var file in files)
            {
                var allLines = new string[] { };
                string line = "";
                try
                {
                    logger.Log($"File Name: {file.FullName}");
                    allLines = File.ReadAllLines(file.FullName);
                    line = allLines[0];
                    //What kind of MNT file is it>

                    if (line.Contains("RANDOMLOCATION"))
                    {
                        logger.Log($"RANDOMLOCATION");
                        ProcessLocation(file);
                    }
                    else if (line.Contains("RANDOMSKU"))
                    {
                        logger.Log($"RANDOMSKU");
                        ProcessRandomSku(file);
                    }
                    else if (line.Contains("OFFCARDEFSKU"))
                    {
                        logger.Log($"OFFCARDEFSKU");
                        ProcessOffCarSku(file);
                    }
                    else if (line.Contains("OFFCARDEFSLOT"))
                    {
                        logger.Log($"OFFCARDEFSLOT");
                        ProcessOffCarLocation(file);
                    }
                    else if (line.Contains("OFFCARRESERVE"))
                    {
                        logger.Log($"OFFCARRESERVE");
                        ProcessOffCarInventory(file);
                    }
                    else if (line.Contains("AKADEFINITION"))
                    {
                        logger.Log($"AKADEFINITION");
                        ProcessAka(file);
                    }
                    else
                    {
                        logger.Log($"Last Else");
                        //if the line doesn't have any of these, it's Inventory
                        // Sku and Location with quantity
                        //ProcessInventory(file);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Reading Maintenance Lines. {ex.Message} \r\n {ex.InnerException}");
                }
            }
        }

        private void ProcessInventory(FileInfo file)
        {
            //var inventoryMaintenanceRecords = new List<InventoryLoad>();
            //bool firstLine = true;
            //var allLines = new string[] { };
            //string line = "";
            //try
            //{
            //    allLines = File.ReadAllLines(file.FullName);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error Reading All Order Lines. {ex.Message} \r\n {ex.InnerException}");
            //}

            //for (int i = 0; i < allLines.Count(); i++)
            //{
            //    line = allLines[i];

            //    var rec = new InventoryLoad();
            //    rec.Station = line.Substring(127,1);
            //    rec.StorageType = line.Substring();
            //    rec.Item = line.Substring();
            //    rec.Description = line.Substring();
            //    rec.Quantity = line.Substring();
            //    rec.Slot = line.Substring();
            //    rec.PrimeBin = line.Substring();
            //    rec.Carousel = line.Substring();
            //    rec.Bin = line.Substring();
            //    rec.Level = line.Substring();
            //    rec.Partition = line.Substring();
            //    rec.Tag = line.Substring();
            //    rec.SizeCode = line.Substring();
            //    rec.VelocityCode = line.Substring();
            //    rec.HeightCode = line.Substring();
            //    rec.UserCode = line.Substring();
            //    rec.ReceivedDate = line.Substring();
            //    rec.Id = line.Substring();

            //    inventoryMaintenanceRecords.Add(rec);

            //}

            ArchiveFile(file);
        }

        private void ProcessAka(FileInfo file)
        {
            var akaRecords = new List<AkaLoad>();
            var akaDefinitionUpdate = new AkaDefinitionUpdate();

            var allLines = new string[] { };
            string line = "";
            try
            {
                allLines = File.ReadAllLines(file.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Process Aka Error Reading All Order Lines. {ex.Message} \r\n {ex.InnerException}");
            }

            for (int i = 0; i < allLines.Count(); i++)
            {
                line = allLines[i];
                logger.Log($"AKA: {line}");
                logger.Log($"Line Length: {line.Length}");
                if (line.Length > 49)
                {
                    string akaSku = line.Substring(50).Trim();
                    logger.Log($"AKA: {akaSku} Length: {akaSku.Length}");
                    string sku = line.Substring(0, 35).Trim();
                    logger.Log($"AKA: {sku} Length: {sku.Length}");
                    if (akaSku.Length > 0 && sku.Length > 0)
                    {
                        var rec = new AkaLoad();
                        rec.AkaSku = akaSku;
                        rec.Item = sku;

                        akaRecords.Add(rec);
                    } 
                }
            }

            akaDefinitionUpdate.ProcessAkaDefinitions(akaRecords, logger);

            ArchiveFile(file);
        }

        private void ProcessOffCarInventory(FileInfo file)
        {
            ArchiveFile(file);
        }

        private void ProcessOffCarLocation(FileInfo file)
        {
            ArchiveFile(file);
        }

        private void ProcessOffCarSku(FileInfo file)
        {
            ArchiveFile(file);
        }

        private void ProcessLocation(FileInfo file)
        {
            ArchiveFile(file);
        }

        private void ProcessRandomSku(FileInfo file)
        {
            var itemMaintenanceRecords = new List<ItemDefinitionLoad>();
            var itemDefinitionUpdate = new ItemDefinitionUpdate();

            var allLines = new string[] { };
            string line = "";
            try
            {
                allLines = File.ReadAllLines(file.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Process Sku Error Reading All Order Lines. {ex.Message} \r\n {ex.InnerException}");
            }
            try
            {

                for (int i = 0; i < allLines.Count(); i++)
                {
                    line = allLines[i];
                    logger.Log($"RandomSku: {line}");

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
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Error Reading ItemDefinitionLoad Lines. {ex.Message} \r\n {ex.InnerException}");
            }

            itemDefinitionUpdate.ProcessItemDefinitions(itemMaintenanceRecords, logger);

            ArchiveFile(file);
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
            string archiveDir = ($"{masterPath.FullName}Archive\\");
            try
            {
                if (File.Exists(fileInfo.FullName))
                {
                    string archivefile = Path.Combine(archiveDir, fileInfo.Name);
                    if (File.Exists($"{archivefile}"))
                    {
                        File.Delete(archivefile);
                    }

                    fileInfo.MoveTo(archivefile);
                }
            }
            catch (Exception ex)
            {
                logger.Log("Archive Master Maintenance File Error: " + ex.Message);
            }
        }

    }

}