using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.UnitOfWorks;
public class LocationUnitOfWork : ILocationUnitOfWork
{
    private readonly DbContext _context;
    private IGenericRepository<SizeCode> _repoSizeCode;
    private IGenericRepository<HeightCode> _repoHeightCode;
    private IGenericRepository<VelocityCode> _repoVelocityCode;
    private GenericRepository<StorageDevice> _repoStorageDevices;
    private GenericRepository<Area> _repoAreas;
    private GenericRepository<LocationCode> _repoLocationCodes;
    private GenericRepository<Location> _repoLocations;

    public LocationUnitOfWork(DbContext context)
    {
        _context = context;
    }
    public IGenericRepository<SizeCode> SizeCodes => _repoSizeCode ??= new GenericRepository<SizeCode>(_context);
    public IGenericRepository<HeightCode> HeightCodes =>
        _repoHeightCode ??= new GenericRepository<HeightCode>(_context);
    public IGenericRepository<Location> Locations =>
        _repoLocations ??= new GenericRepository<Location>(_context);
    public IGenericRepository<LocationCode> LocationCodes =>
        _repoLocationCodes ??= new GenericRepository<LocationCode>(_context);
    public IGenericRepository<VelocityCode> VelocityCodes =>
        _repoVelocityCode ??= new GenericRepository<VelocityCode>(_context);
    public IGenericRepository<StorageDevice> StorageDevices =>
        _repoStorageDevices ??= new GenericRepository<StorageDevice>(_context);
    public IGenericRepository<Area> Areas =>
        _repoAreas ??= new GenericRepository<Area>(_context);

    public void Dispose()
    {
        _context.Dispose();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
