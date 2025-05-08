using APBD_CW_7_W.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_CW_7_W.Controllers;


[ApiController]
[Route("api/trips")]
public class TripsController (IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> getAllTrips()
    {
        return Ok(await service.GetAllTripsWithCountryAsync());
    }
}