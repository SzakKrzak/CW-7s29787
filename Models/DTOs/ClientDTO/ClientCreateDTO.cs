using System.ComponentModel.DataAnnotations;

namespace APBD_CW_7.Models.DTOs;

public class ClientCreateDTO
{
    [Required] [MaxLength(120)] 
    public string FirstName { get; set; }
    
    [Required] 
    [MaxLength(120)] 
    public string LastName { get; set; }
    
    [Required] 
    [MaxLength(120)] [EmailAddress] 
    public string Email { get; set; }
    
    [Required] 
    [MaxLength(120)] 
    public string Telephone { get; set; }
    
    [Required] 
    [MaxLength(120)] [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be 11 digits.")] 
    public string Pesel { get; set; }
}