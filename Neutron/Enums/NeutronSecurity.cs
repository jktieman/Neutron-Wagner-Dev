using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Enums
{
    public enum NeutronSecurity
    {
        [Description("Manage Inventory")]
        ManageInventory = 1,
        [Description("Manage Items")]
        ManageItems = 2,
        [Description("Manage Locations")]
        ManageLocations = 3,
        [Description("View History")]
        ViewHistory = 4,
        [Description("Allow Hot Actions")]
        HotActions = 5,
        [Description("Pick Items and Orders")]
        PickItemsandOrders = 6,
        [Description("Store Items and Orders")]
        StoreItemsandOrders = 7,
        [Description("Manage Users")]
        ManageUsers = 8,
        [Description("Manage Utilities")]
        ManageUtilities = 9,
        [Description("Manage System")]
        ManageSystem = 10,
        [Description("Manage Location Access Control")]
        ManageLac = 11,
    }
}
