using System;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;

namespace NeutronData.UnitOfWorks;
public interface ILocationUnitOfWork : IDisposable
{
    IGenericRepository<SizeCode> SizeCodes { get; }
    IGenericRepository<HeightCode> HeightCodes { get; }
    IGenericRepository<VelocityCode> VelocityCodes { get; }
    IGenericRepository<LocationCode> LocationCodes { get; }
    IGenericRepository<Location> Locations { get; }
    IGenericRepository<StorageDevice> StorageDevices { get; }
    IGenericRepository<Area> Areas { get; }
    void Save();

}
