using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class CdefProcessor
    {
        private IList<CdefRec> cdefRecords = new List<CdefRec>();

        public CdefProcessor()
        {
        }
        
        public IList<CdefRec> GetCdefRecords()
        {
            Init();
            return cdefRecords;
        }

        public CdefRec Get(string sku)
        {
            if (cdefRecords.Count == 0)
            {
                Init();
            }
            return cdefRecords.Where(c => c.Sku.Trim() == sku.Trim()).FirstOrDefault();
        }


        public IDefRec GetFromNova(string sku)
        {
            CdefRec cdefRec = null;
            try
            {
                var result = SafeNativeMethods.GETCDEFRECORD(ref sku);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    string[] values = result.Split('|');
                    cdefRec = new CdefRec();
                    cdefRec.Sku = values[0];
                    cdefRec.Des = values[1];
                    cdefRec.SysCap = int.Parse(values[2]);
                    cdefRec.SysTrig = int.Parse(values[3]);
                    cdefRec.Station = int.Parse(values[4]);
                    cdefRec.LocCap = int.Parse(values[5]);
                    cdefRec.LocTrig = int.Parse(values[6]);
                    cdefRec.SizeClass = byte.Parse(values[7]);
                    cdefRec.VelClass = byte.Parse(values[8]);
                    cdefRec.OcOnly = byte.Parse(values[9]);
                    return cdefRec;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to retrieve Carousel Definition.\r\n " + ex.Message);
            }
            return cdefRec;
        }

        public bool Save(CdefRec cdefRec)
        {
            bool result = false;
            if (Validate(cdefRec))
            {
                string rec = cdefRec.ToCsv();
                try
                {
result = SafeNativeMethods.SAVECDEFRECORD(ref rec);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save Carousel Definition.\r\n" + ex.Message);
               }
                
            }
            
            return result;

        }

        private bool Validate(CdefRec cdefRec)
        {
            if (cdefRec.Sku.Length > 0)
            {
                if (cdefRec.Des.Length > 0 && cdefRec.Des.Length <= 30)
                {
                    if (cdefRec.SysCap >= 0)
                    {
                        if (cdefRec.SysTrig >= 0)
                        {
                            if (cdefRec.Station > 0 && cdefRec.Station < 10)
                            {
                                return true;
                            }
                        }
                    }
                }else
                {
                    MessageBox.Show("Check Description Length. " + cdefRec.Des.Length.ToString() + " characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return false;
        }

        public void Init()
        {
            cdefRecords = new List<CdefRec>();
            try
            {
                string[] lineDetail;
                string allRecs = SafeNativeMethods.GETCDEFRECS();

                //todo - don't split allRecs every time create an array to loop over

                foreach (var line in allRecs.SplitToLines().ToArray())
                {
                    lineDetail = line.Split('|');
                    var c = new CdefRec(lineDetail[0]
                        , lineDetail[1]
                        , int.Parse(lineDetail[2])
                        , int.Parse(lineDetail[3])
                        , int.Parse(lineDetail[4])
                        , int.Parse(lineDetail[5])
                        , int.Parse(lineDetail[6])
                        , byte.Parse(lineDetail[7])
                        , byte.Parse(lineDetail[8])
                        , 0
                        );
                    cdefRecords.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ERROR - Cannot initialize CdefRecords. {0}", ex.Message));
            }
        }
    }
}
