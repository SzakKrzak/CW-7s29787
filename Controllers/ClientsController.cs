using APBD_CW_7_W.Services;
using APBD_CW_7.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace APBD_CW_7_W.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IDbService service) : ControllerBase
{
    [HttpGet("{id}/trips")]
    public async Task<ActionResult> getClients(int id)
    {
        return Ok(await service.GetAllClientsTrips(id));
    }

    [HttpPost]
    public async Task<ActionResult> postClient([FromBody] ClientCreateDTO client)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var clientId = await service.AddClientAsync(client);

        if (clientId == null)
        {
            return StatusCode(500, "There was an error while adding new client");
        }

        return Created("",clientId);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<ActionResult> putClients(int id, int tripId)
    {
        var result = await service.RegisterClientForTripAsync(id, tripId);

        return result switch
        {
            null => Ok("Client registered for the trip successfully."),
            "Client not found" => NotFound("Client does not exist."),
            "Trip not found" => NotFound("Trip does not exist."),
            "Trip is full" => BadRequest("The trip has reached maximum capacity."),
            _ => StatusCode(500, "Unexpected error: " + result)
        };
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<ActionResult> DeleteRegistration(int id, int tripId)
    {
        var result = await service.DeleteClientAsync(id, tripId);
        return result switch
        {
            null => Ok("Registery for the trip successfully deleted."),
            "Registration does not exist" => NotFound("Registration does not exist"),
            _ => StatusCode(500, "Unexpected error: " + result)
        };
    }
}