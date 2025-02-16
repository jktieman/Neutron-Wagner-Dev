using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace NeutronData.UnitOfWorks;
public class LocationUnitOfWork : ILocationUnitOfWork
{
    private readonly Func<NeutronDb> _contextFactory;
    private IGenericRepository<SizeCode> _repoSizeCode;
    private IGenericRepository<HeightCode> _repoHeightCode;
    private IGenericRepository<VelocityCode> _repoVelocityCode;
    private GenericRepository<StorageDevice> _repoStorageDevices;
    private GenericRepository<Area> _repoAreas;
    private GenericRepository<LocationCode> _repoLocationCodes;
    private GenericRepository<Location> _repoLocations;

    public LocationUnitOfWork(Func<NeutronDb> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }
    public IGenericRepository<SizeCode> SizeCodes => _repoSizeCode ??= new GenericRepository<SizeCode>(_contextFactory);
    public IGenericRepository<HeightCode> HeightCodes =>
        _repoHeightCode ??= new GenericRepository<HeightCode>(_contextFactory);
    public IGenericRepository<Location> Locations =>
        _repoLocations ??= new GenericRepository<Location>(_contextFactory);
    public IGenericRepository<LocationCode> LocationCodes =>
        _repoLocationCodes ??= new GenericRepository<LocationCode>(_contextFactory);
    public IGenericRepository<VelocityCode> VelocityCodes =>
        _repoVelocityCode ??= new GenericRepository<VelocityCode>(_contextFactory);
    public IGenericRepository<StorageDevice> StorageDevices =>
        _repoStorageDevices ??= new GenericRepository<StorageDevice>(_contextFactory);
    public IGenericRepository<Area> Areas =>
        _repoAreas ??= new GenericRepository<Area>(_contextFactory);

    public void Save()
    {
        using var context = _contextFactory();
        context.SaveChanges();
    }

    public void Dispose()
    {
        _repoSizeCode = null;
        _repoHeightCode = null;
        _repoVelocityCode = null;
        _repoStorageDevices = null;
        _repoAreas = null;
        _repoLocationCodes = null;
        _repoLocations = null;
    }
}
