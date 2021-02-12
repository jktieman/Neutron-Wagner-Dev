using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Models
{
    public class CostCenterManager
    {
        private readonly string _costCenterFile;
        private readonly List<CostCenter> _costCenterList;
        private List<string> _ccList; 

        public CostCenterManager(string costCenterFile)
        {
            _costCenterFile = costCenterFile;
            _costCenterList = new List<CostCenter>();
        }

        public async Task<List<CostCenter>> GetCostCenterListAsync()
        {
            try
            {
                if (File.Exists(_costCenterFile))
                {
                    _ccList = await Task.Run(() => File.ReadAllLines(_costCenterFile).ToList());

                    _costCenterList.Add(new CostCenter {Code = string.Empty,Name = string.Empty});
                    foreach (var cc in _ccList)
                    {
                        if (cc.Length <= 11) continue;
                        var costCenter = new CostCenter()
                        {
                            Code = cc.Substring(0, 10).Trim(),
                            Name = cc.Substring(0, 10).Trim() + " - " + cc.Substring(10).Trim()
                        };
                        _costCenterList.Add(costCenter);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cost Center File Error.  {ex.Message} \r\n {ex.InnerException}");
            }
            return _costCenterList;
        }

        public List<CostCenter> GetCostCenterList(string search)
        {
            var recs = _costCenterList.Where(c => c.Name.ToLower().Contains(search))
                .Select(s => new CostCenter { Code = s.Code, Name = s.Name}).ToList();

            return recs;
        }
    }


    public class CostCenter
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
