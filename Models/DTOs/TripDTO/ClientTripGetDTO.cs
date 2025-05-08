namespace APBD_CW_7.Models.DTOs;

public class ClientTripGetDTO
{
    public int IdTrip { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}