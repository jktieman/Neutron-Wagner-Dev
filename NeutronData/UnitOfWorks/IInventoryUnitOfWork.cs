using System;
using NeutronData.Models.Lookups;
using NeutronData.Models;
using NeutronData.Repositories;

namespace NeutronData.UnitOfWorks;
public interface IInventoryUnitOfWork : IDisposable
{
    IGenericRepository<SizeCode> SizeCodes { get; }
    IGenericRepository<HeightCode> HeightCodes { get; }
    IGenericRepository<VelocityCode> VelocityCodes { get; }
    IGenericRepository<LocationCode> LocationCodes { get; }
    IGenericRepository<Location> Locations { get; }
    IGenericRepository<ItemDefinition> ItemDefinitions { get; }
    IGenericRepository<StorageDevice> StorageDevices { get; }
    IGenericRepository<Area> Areas { get; }
    IGenericRepository<UnitOfIssue> UnitOfIssue { get; }
    IGenericRepository<Inventory> Inventory { get; }
    IGenericRepository<StorageType> StorageTypes { get; }

}
