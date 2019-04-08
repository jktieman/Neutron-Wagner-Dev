using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class OcDefProcessor
    {
        private IList<OcDefRec> ocDefRecords = new List<OcDefRec>();
        
        public OcDefProcessor()
        {
        }

        public IList<OcDefRec> GetOcDefRecords()
        {
            Init();
            return ocDefRecords;
        }

        public OcDefRec Get(string sku)
        {
            if (ocDefRecords.Count == 0)
            {
                Init();
            }
            return ocDefRecords.Where(c => c.Sku.Trim() == sku.Trim()).FirstOrDefault();
        }

        public IDefRec GetFromNova(string sku)
        {
            var result = SafeNativeMethods.GETOCDEFRECORD(ref sku);
            if (!string.IsNullOrWhiteSpace(result))
            {
                string[] values = result.Split('|');
                OcDefRec def = new OcDefRec();
                def.Sku = values[0];
                def.Des = values[1];
                def.SysCap = int.Parse(values[2]);
                def.SysTrig = int.Parse(values[3]);
                return def;
            }
            return null;
        }

        public bool Save(OcDefRec ocDefRec)
        {
            bool result = false;
            if (Validate(ocDefRec))
            {
                string rec = ocDefRec.ToCsv();
                result = SafeNativeMethods.SAVEOCDEFRECORD(ref rec);
            }

            return result;
        }
 
        private bool Validate(OcDefRec ocDefRec)
        {
            if (ocDefRec.Sku.Length > 0)
            {
                if (ocDefRec.Des.Length > 0 && ocDefRec.Des.Length <= 30)
                {
                    if (ocDefRec.SysCap >= 0)
                    {
                        if (ocDefRec.SysTrig >= 0)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Check Description Length. " + ocDefRec.Des.Length.ToString() + " characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return false;
        }

        public void Init()
        {
            ocDefRecords = new List<OcDefRec>();
            string allRecs = string.Empty;
            try
            {
                string[] lineDetail;

                allRecs = SafeNativeMethods.GETOCDEFRECS();

                foreach (var line in allRecs.SplitToLines().ToArray())
                {
                    lineDetail = line.Split('|');
                    var c = new OcDefRec(lineDetail[0], lineDetail[1], int.Parse(lineDetail[2]), int.Parse(lineDetail[3]));
                    ocDefRecords.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ERROR - Cannot initialize OcDefRecords. {0}", ex.Message));
            }
        }
    }
}
