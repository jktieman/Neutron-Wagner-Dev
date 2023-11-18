using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronData.Models.Lookups;
using NeutronCore.Enums;
using System.Data.SqlClient;
using StationType = NeutronCore.Enums.StationType;
using Logger = NeutronCore.Global.Logger;
using NeutronCore.Global;

namespace NeutronData.Repositories;

public class AreaRepository : IAreaRepository
{
    private const int Ebin = 4;

    private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
    private readonly GenericRepository<Workstation> _repoWorkstation = new GenericRepository<Workstation>(new NeutronDb());
    private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
    private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
    private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());

    private readonly IDynamicLogger _logger;

    private readonly GenericRepository<Area> _repoArea = new GenericRepository<Area>(new NeutronDb());

    public AreaRepository()
    {
        _logger = Logger.SetupLogger("AreaRepository");
    }

    public int GetAreaId(int areaNumber)
    {
        var areaId = 0;
        var result = _repoArea.FindBy(r => r.AreaNumber == areaNumber).FirstOrDefault();
        if (result != null)
        {
            areaId = result.Id;
        }
        return areaId;
    }

    public Area GetArea(int id)
    {
        return _repoArea.FindByKey(id);
    }
    public List<Area> Lookup()
    {
        var areas = _repoArea.All().OrderBy(o => o.AreaNumber).ToList();
        return areas;
    }
    public int[] GetAllAreaIds()
    {
        return _repoArea.All().Select(r => r.Id).ToArray();
    }

    public int[] GetAllPickableAreaIds()
    {
        var rec = new int[]{};
        try
        {
            rec = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.Id).ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return rec;

    }

    public List<Area> GetAllAreas()
    {
        var recs = new List<Area>();
        try
        {
            recs = _repoArea.All().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return recs;
    }

    public List<Area> GetAllPickableAreas()
    {
        var recs = new List<Area>();
        try
        {
            recs = _repoArea.All().Where(r => r.Pickable == true).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return recs;
    }

    public List<int> GetAllPickableAreaNumbers()
    {
        var recs = new List<int>();
        try
        {
            recs = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.AreaNumber).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return recs;
    }

    public List<string> GetAllPickableAreaNumbersAsString()
    {
        var recs = new List<string>();
        try
        {
            recs = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.AreaNumber.ToString()).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return recs;
    }
}