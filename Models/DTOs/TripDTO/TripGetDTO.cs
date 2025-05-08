namespace APBD_CW_7.Models.DTOs;

public class TripGetDTO
{
    public int IdTrip { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public String Countries { get; set; }
}