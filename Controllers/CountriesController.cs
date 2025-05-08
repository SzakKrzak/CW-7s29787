using APBD_CW_7_W.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_CW_7_W.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> getAllCountries()
    {
        return Ok(await service.GetAllTripsWithCountryAsync());
    }
}