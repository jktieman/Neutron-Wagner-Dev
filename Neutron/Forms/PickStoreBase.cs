using MetroFramework.Forms;
using Neutron.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Forms
{
    public static class PickStoreBase 
    {
        /// <summary>
        /// Initializes the list of orders to be picked.
        /// </summary>
        /// <param name="pickBatchSize">The size of the batch to be picked.</param>
        public static List<BatchPosition> InitOrdersToPick(int pickBatchSize)
        {
            var ordersToPick = new List<BatchPosition>();
            for (var index = 0; index < pickBatchSize; index++)
            {
                var batchPosition = new BatchPosition
                {
                    PositionNumber = index + 1,
                    OrderId = 0,
                    Ord1 = string.Empty,
                    Ord2 = string.Empty,
                    OrderComplete = false
                };
                ordersToPick.Add(batchPosition);
            }
            return ordersToPick;
        }
    }
}
