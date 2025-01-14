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
    private readonly GenericRepository<Area> _repoArea = new GenericRepository<Area>(new NeutronDb());

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

    /// <summary>
    /// Retrieves a list of all pickable area numbers as strings.
    /// </summary>
    /// <returns>A list of strings representing the pickable area numbers.</returns>
    /// <exception cref="Exception">Thrown when an error occurs during the retrieval process.</exception>
    public List<string> GetAllPickableAreaNumbersAsString()
    {
        try
        {
            return _repoArea.All()
                .Where(r => r.Pickable)
                .Select(r => r.AreaNumber.ToString())
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<string>();
        }
    }

}