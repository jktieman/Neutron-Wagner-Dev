using NeutronData.Models.Lookups;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.UnitOfWorks;
public interface IInventoryUnitOfWork
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
    
    
    void Save();

}
