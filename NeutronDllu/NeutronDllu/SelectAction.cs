using System.Drawing;
using System.Windows.Forms;
using NeutronData.ModelViews;


namespace NeutronDllu
{
    public static class SelectAction
    {
        /// <summary>
        /// Called when the Accept button is pressed on the Order Selection Screen and before any other processing happens.
        ///  </summary>
        /// <param name="pickStop">Passes in the entire <see cref="PickStop"/> object.</param>
        /// <returns>Return true to continue processing and false to exit processing</returns>
        public static bool Accept(PickStop pickStop)
        {
            return pickStop != null;
        }

        /// <summary>
        /// Called when the Accept button is pressed on the Replenishment Screen and before any other processing happens.
        ///  </summary>
        /// <param name="pickStop">Passes in the entire <see cref="ReplenPickStop"/> object.</param>
        /// <returns>Return true to continue processing and false to exit processing</returns>
        public static bool StoreAccept(ReplenPickStop pickStop)
        {
            var result = true;
            if (pickStop == null) return false;

            return result;
        }

    }
}
