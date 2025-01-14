using NeutronData.Models.Lookups;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.UnitOfWorks;
public class InventoryUnitOfWork : IInventoryUnitOfWork
{
    private readonly DbContext _context;
    private IGenericRepository<SizeCode> _repoSizeCode;
    private IGenericRepository<HeightCode> _repoHeightCode;
    private IGenericRepository<VelocityCode> _repoVelocityCode;
    private GenericRepository<StorageDevice> _repoStorageDevices;
    private GenericRepository<Area> _repoAreas;
    private GenericRepository<LocationCode> _repoLocationCodes;
    private GenericRepository<Location> _repoLocations;
    private GenericRepository<ItemDefinition> _repoItemDefinitions;
    private GenericRepository<UnitOfIssue> _repoUnitOfIssue;
    private IGenericRepository<Inventory> _repoInventory;
    private GenericRepository<StorageType> _repoStorageTypes;


    public InventoryUnitOfWork(DbContext context)
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
    public IGenericRepository<UnitOfIssue> UnitOfIssue =>
        _repoUnitOfIssue ??= new GenericRepository<UnitOfIssue>(_context);
    public IGenericRepository<ItemDefinition> ItemDefinitions =>
        _repoItemDefinitions ??= new GenericRepository<ItemDefinition>(_context);

    public IGenericRepository<Inventory> Inventory => 
        _repoInventory ??= new GenericRepository<Inventory>(_context);

    public IGenericRepository<StorageType> StorageTypes =>
        _repoStorageTypes ??= new GenericRepository<StorageType>(_context);

    public void Dispose()
    {
        _context.Dispose();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
