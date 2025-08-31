using System;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.ModelViews;

namespace Neutron.Forms
{
    /// <summary>
    /// Represents a form for adjusting the view of a pick operation.
    /// </summary>
    /// <remarks>
    /// This form provides a user interface for adjusting the details of a pick operation
    /// , such as the pick position and the quantity to be picked.
    /// </remarks>
    public partial class FrmPickViewAdjustment : Form
    {
        private readonly PickView _pickView;
        private readonly IDynamicLogger _logger;
        public bool Success;
        public string ButtonPressed = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmPickViewAdjustment"/> class.
        /// </summary>
        /// <param name="pickView">The pick view model that contains the details of the pick operation.</param>
        /// <param name="logger"></param>
        /// <remarks>
        /// This constructor initializes the form with the details of the pick operation from the provided pick view model.
        /// It sets up the form fields such as order, item, description, pick position
        /// , and quantity to be picked with the corresponding values from the pick view model.
        /// </remarks>
        public FrmPickViewAdjustment(PickView pickView, IDynamicLogger logger)
        {
            InitializeComponent();

            KeyPreview = true;
            _pickView = pickView;
            _logger = logger;
            LabelOrder.Text = pickView.Ord1;
            LabelItem.Text = pickView.Item;
            LabelDescription.Text = pickView.Description;
            TextBoxPosition.Text = pickView.PickPosition.ToString();
            TextBoxNewQuantity.Text = pickView.QuantityToBePicked.ToString();
        }
        /// <summary>
        /// Handles the Click event of the ButtonHighlight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonHighlight_Click(object sender, EventArgs e)
        {
            Highlight();
        }
        /// <summary>
        /// Highlights the current pick operation and adjusts its status and quantities.
        /// </summary>
        /// <remarks>
        /// This method sets the status of the current pick operation to 'Skipped', resets the picked quantity to zero,
        /// and updates the quantity to be picked to the total quantity of the order. It also sets the 'Success' flag to true
        /// and closes the form.
        /// </remarks>
        private void Highlight()
        {
            ButtonPressed = "Highlight";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Skipped;
            _pickView.OrderDetail.PickedQuantity = 0;
            _pickView.PickedQty = 0;
            _pickView.QuantityToBePicked = _pickView.Quantity;
            _logger.LogDetailAsync($"Skip Item: {_pickView.Item}");
            Success = true;
            Close();
        }
        /// <summary>
        /// Handles the Click event of the Backorder button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonBackorder_Click(object sender, EventArgs e)
        {
            Backorder();
        }
        /// <summary>
        /// Sets the status of the order line to 'Complete' and updates the picked quantity.
        /// This method is typically called when the 'Backorder' button is clicked or the 'Down' key is pressed.
        /// </summary>
        private void Backorder()
        {
            ButtonPressed = "Backorder";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;

            _pickView.OrderDetail.PickedQuantity = TextBoxNewQuantity.Text.ParseInt();  // _pickView.PickedQty;
            _pickView.PickedQty =  TextBoxNewQuantity.Text.ParseInt();
            _logger.LogDetailAsync($"Backorder: {_pickView.Item} Quantity: {_pickView.PickedQty}");
            //_pickView.QuantityToBePicked = _pickView.Quantity;
            Success = true;
            Close();
        }
        /// <summary>
        /// Handles the Click event of the Accept button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            Accept();
        }
        /// <summary>
        /// Sets the status of the current order to complete and updates the picked quantity.
        /// </summary>
        /// <remarks>
        /// This method is called when the Accept button is pressed. It updates the LineStatusId of the order detail to Complete,
        /// sets the PickedQuantity to the QuantityToBePicked, and updates the PickedQty of the pick view. 
        /// The method also sets the Success flag to true and closes the form.
        /// </remarks>
        private void Accept()
        {
            ButtonPressed = "Accept";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
            _pickView.OrderDetail.PickedQuantity = TextBoxNewQuantity.Text.ParseInt();   // _pickView.QuantityToBePicked;
            _pickView.PickedQty = TextBoxNewQuantity.Text.ParseInt();   // _pickView.QuantityToBePicked;
            _logger.LogDetailAsync($"Accept: {_pickView.Item} Quantity: {_pickView.PickedQty}");
            Success = true;
            Close();
        }
        /// <summary>
        /// Handles the KeyDown event of the FrmPickViewAdjustment form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method performs different actions based on the key pressed:
        /// - Left key: Highlights the current selection.
        /// - Down key: Marks the current selection for backorder.
        /// - Right key: Accepts the current selection.
        /// </remarks>
        private void FrmPickViewAdjustment_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    {
                        Highlight();
                        break;
                    }
                case Keys.Down:
                    {
                        Backorder();
                        break;
                    }
                case Keys.Right:
                    {
                        Accept();
                        break;
                    }
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
