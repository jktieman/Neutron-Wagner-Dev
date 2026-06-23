# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Neutron is a Windows Forms warehouse management system (WMS) built on .NET Framework 4.8. It manages pick/store operations, replenishment, inventory, and hardware device integrations for automated storage systems.

**Solution file:** `NeutronProject-DEV.sln`  
**Output executable:** `Neutron\bin\Debug\NeutronTest.exe` (x86 Debug) or `Neutron\bin\Release\NeutronTest.exe` (x64 Release)

## Build Commands

Open in Visual Studio 2022 and build via the IDE, or use MSBuild from the repository root:

```powershell
# Debug (x86)
msbuild NeutronProject-DEV.sln /p:Configuration=Debug /p:Platform=AnyCPU

# Release (x64)
msbuild NeutronProject-DEV.sln /p:Configuration=Release /p:Platform=AnyCPU
```

Individual sub-projects each have their own `.sln` files under their directories and can be built independently when working on isolated modules.

## Architecture

### Layer Structure

| Layer | Projects |
|-------|----------|
| UI (WinForms) | `Neutron` |
| Business Logic | `NeutronCore`, `NeutronEvents`, `NeutronLoader` |
| Data Access | `NeutronData` |
| Device Integration | `IPTI`, `HanelCommands`, `Hanel_DC`, `ProLiteController`, `EthernetTransmitter` |
| Services | `AlliedPostOffice`, `AlliedLogger`, `AlliedFileSystemWatcher`, `ReplenService`, `SlotNameFactory` |
| UI Controls | `NeutronUserControls`, `DGVPrinter`, `EBinDisplayManager` |

### Core Projects

**`Neutron/`** — Main WinForms application (entry point: `Program.cs` → `FrmMain`)
- Forms in `Neutron/Forms/` (FrmMain, FrmPick, FrmReplen, FrmInventory, FrmSystem, etc.)
- Ninject DI configured in `Neutron/Ninject/DI.cs`; use `DI.Create<T>()` for object creation throughout the UI layer
- Controllers in `Neutron/Controllers/` (BayController, InterfaceController, etc.)

**`NeutronCore/`** — Shared enums, extensions, models, and globals
- Key enums: `ActionCode`, `DeviceTypeEnum`, `LineStatus`, `LocationTypeEnum`, `OrderStatus`, `StationType`, `StorageType`
- `NeutronCore/Global/NeutronVariables.cs` — application-wide config loaded from JSON at startup
- `NeutronCore/Global/Logger.cs` — logging wrapper

**`NeutronData/`** — Entity Framework 6 data access (Code First)
- Two DB contexts: `NeutronDb` (main warehouse data) and `SecureDb` (users/roles/permissions)
- Repository pattern via `GenericRepository<T>` plus entity-specific repositories under `NeutronData/Repositories/`
- Unit of Work: `InventoryUnitOfWork`, `LocationUnitOfWork`
- ModelViews (e.g., `PickView`, `PickStop`, `ReplenOrderView`) are read-optimized projections used for display
- Migrations live in `NeutronData/Migrations/`; run `Update-Database` in Package Manager Console targeting `NeutronData`

### Startup Flow

1. `Program.Main()` acquires a named Mutex to prevent duplicate instances
2. `DI.Initialize()` builds the Ninject kernel
3. JSON config loaded: `NeutronVariables` (settings) and `NeutronLicense` (company code)
4. Both `NeutronDb` and `SecureDb` connections checked
5. If connections succeed → `FrmMain`; otherwise → `FrmSystem` (setup/config mode)

### Dependency Injection

Ninject is the DI container. All resolvable types must be bound in the Ninject module(s) loaded in `DI.Initialize()`. Use `DI.Create<T>(arg1, arg2)` for constructor injection with parameters.

### Key Technologies

- **ORM:** Entity Framework 6 (Code First, migrations)
- **DI:** Ninject 3.3.6
- **UI theme:** MetroModernUI 1.4.0.0
- **Logging:** AlliedLogger (custom, wraps file-based logging)
- **JSON config:** JsonManager + Newtonsoft.Json 13.0.3
- **Serial comms:** RJCP.SerialPortStream 3.0.0
- **Async helpers:** AsyncAwaitBestPractices 9.0.0

### Configuration Files

Runtime configuration is JSON-based, loaded by `JsonManager` from a root directory resolved via `INeutronRootDirectory`. `NeutronVariables` controls station behavior, language (`DefaultLanguage` as a culture string like `"en-US"`), and hardware settings. `NeutronLicense` holds the company code.

### Database Migrations

To add a migration (run in Package Manager Console with `NeutronData` as the default project):

```powershell
Add-Migration "MigrationName"
Update-Database
```

### Branch Naming Context

The current branch `Replen_Two_Stations` relates to replenishment logic that spans multiple pick stations — relevant code is primarily in `Neutron/Forms/` (replenishment forms), `NeutronData/Repositories/`, and `ReplenService/`.

### Legacy Code

The `HighJump/` directory contains an archived copy of the previous WMS integration (`Neutron-Wagner-PROD_old`) and is not part of the active build.
