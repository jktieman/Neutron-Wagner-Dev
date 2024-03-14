namespace NeutronData.Interfaces;

public interface IOrderDetail
{
    int PickedQuantity { get; set; }
    int LineStatusId { get; set; }
}