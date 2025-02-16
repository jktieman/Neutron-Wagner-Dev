using NeutronData.Models.Lookups;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using NeutronData.DataContexts;

namespace NeutronData.UnitOfWorks;
public class InventoryUnitOfWork : IInventoryUnitOfWork
{
    private readonly Func<NeutronDb> _contextFactory;
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


    public InventoryUnitOfWork(Func<NeutronDb> contextFactory)
    {
        _contextFactory = contextFactory;

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
    public IGenericRepository<UnitOfIssue> UnitOfIssue =>
        _repoUnitOfIssue ??= new GenericRepository<UnitOfIssue>(_contextFactory);
    public IGenericRepository<ItemDefinition> ItemDefinitions =>
        _repoItemDefinitions ??= new GenericRepository<ItemDefinition>(_contextFactory);

    public IGenericRepository<Inventory> Inventory => 
        _repoInventory ??= new GenericRepository<Inventory>(_contextFactory);

    public IGenericRepository<StorageType> StorageTypes =>
        _repoStorageTypes ??= new GenericRepository<StorageType>(_contextFactory);

    public void Dispose()
    {
        // Dispose of any repositories that implement IDisposable
        _repoSizeCode = null;
        _repoHeightCode = null;
        _repoVelocityCode = null;
        _repoStorageDevices = null;
        _repoAreas = null;
        _repoLocationCodes = null;
        _repoLocations = null;
        _repoItemDefinitions = null;
        _repoUnitOfIssue = null;
        _repoInventory = null;
        _repoStorageTypes = null;
    }
}
