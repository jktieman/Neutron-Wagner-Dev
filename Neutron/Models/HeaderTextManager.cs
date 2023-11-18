using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using NeutronData.ModelViews;
using static System.Net.Mime.MediaTypeNames;

namespace Neutron.Models
{
    public class HeaderTextManager
    {
        public string GetHeaderText(WorkstationView workstationView, string header, ResourceManager gridResourceManager)
        {
            if (workstationView is null || header is null) { return string.Empty; }

            switch (header)
            {
                // headerText = _workstationView.StationTypeId == (int)NeutronCore.Enums.StationType.RackTablet
                // ? "Zone" : _gridResourceManager.GetString(text);

                case "Loc1":
                    {
                        switch (workstationView.StationTypeId)
                        {
                            case 1: // Carousel
                                {
                                    return gridResourceManager.GetString("Carousel_Loc1");
                                }
                            case 2: // Vertical
                                {
                                    return gridResourceManager.GetString("Vertical_Loc1");
                                }
                            case 3: // Rack
                                {
                                    return gridResourceManager.GetString("Rack_Loc1");
                                }
                            case 5: // EBin
                                {
                                    return gridResourceManager.GetString("EBin_Loc1");
                                }
                            case 7: // Blastzone
                            {
                                return gridResourceManager.GetString("Blastzone_Loc1");
                            }
                            default:
                                {
                                return gridResourceManager.GetString("Rack_Loc1");
                                }
                        }
                    }
                case "Loc2":
                    {
                        switch (workstationView.StationTypeId)
                        {
                            case 1: // Carousel
                                {
                                    return gridResourceManager.GetString("Carousel_Loc2");
                                }
                            case 2: // Vertical
                                {
                                    return gridResourceManager.GetString("Vertical_Loc2");
                                }
                            case 3: // Rack
                                {
                                    return gridResourceManager.GetString("Rack_Loc2");
                                }
                            case 5: // EBin
                                {
                                    return gridResourceManager.GetString("EBin_Loc2");
                                }
                            case 7: // Blastzone
                            {
                                return gridResourceManager.GetString("Blastzone_Loc2");
                            }
                            default:
                            {
                                return gridResourceManager.GetString("Rack_Loc2");
                            }
                        }
                    }
                case "Loc3":
                    {
                        switch (workstationView.StationTypeId)
                        {
                            case 1: // Carousel
                                {
                                    return gridResourceManager.GetString("Carousel_Loc3");
                                }
                            case 2: // Vertical
                                {
                                    return gridResourceManager.GetString("Vertical_Loc3");
                                }
                            case 3: // Rack
                                {
                                    return gridResourceManager.GetString("Rack_Loc3");
                                }
                            case 5: // EBin
                                {
                                    return gridResourceManager.GetString("EBin_Loc3");
                                }
                            case 7: // Blastzone
                            {
                                return gridResourceManager.GetString("Blastzone_Loc3");
                            }
                            default:
                            {
                                return gridResourceManager.GetString("Rack_Loc3");
                            }
                        }
                    }
                case "Loc4":
                    {
                        switch (workstationView.StationTypeId)
                        {
                            case 1: // Carousel
                                {
                                    return gridResourceManager.GetString("Carousel_Loc4");
                                }
                            case 2: // Vertical
                                {
                                    return gridResourceManager.GetString("Vertical_Loc4");
                                }
                            case 3: // Rack
                                {
                                    return gridResourceManager.GetString("Rack_Loc4");
                                }
                            case 5: // EBin
                                {
                                    return gridResourceManager.GetString("EBin_Loc4");
                                }
                            case 7: // Blastzone
                            {
                                return gridResourceManager.GetString("Blastzone_Loc4");
                            }
                            default:
                            {
                                return gridResourceManager.GetString("Rack_Loc4");
                            }
                        }
                    }
                case "Loc5":
                    {
                        switch (workstationView.StationTypeId)
                        {
                            case 1: // Carousel
                                {
                                    return gridResourceManager.GetString("Carousel_Loc5");
                                }
                            case 2: // Vertical
                                {
                                    return gridResourceManager.GetString("Vertical_Loc5");
                                }
                            case 3: // Rack
                                {
                                    return gridResourceManager.GetString("Rack_Loc5");
                                }
                            case 5: // EBin
                                {
                                    return gridResourceManager.GetString("EBin_Loc5");
                                }
                            case 7: // Blastzone
                            {
                                return gridResourceManager.GetString("Blastzone_Loc5");
                            }
                            default:
                            {
                                return gridResourceManager.GetString("Rack_Loc5");
                            }
                        }
                    }
            }
            return string.Empty;
        }
    }
}
