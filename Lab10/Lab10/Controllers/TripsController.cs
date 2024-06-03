using Lab10.Data;
using Lab10.Models;
using Lab10.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;
    
    public TripsController(ApbdContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if(page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize parameters must be greater than 0");
        }

        var totalTrips = await _context.Trips.CountAsync();
        var allPages = (totalTrips + pageSize - 1) / pageSize;

        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation) // Assuming IdClientNavigation is the navigation property for Client
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Name = e.Name,
                Description = e.Description,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxPeople = e.MaxPeople,
                Countries = e.IdCountries.Select(c => new
                {
                    Name = c.Name
                }),
                Clients = e.ClientTrips.Select(ct => new
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                })
            })
            .ToListAsync();

        var result = new
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = allPages,
            trips = trips
        };

        return Ok(result);
    }
    
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients.FindAsync(idClient);
        if (client == null)
        {
            return NotFound();
        }

        var hasTrips = await _context.ClientTrips.AnyAsync(ct => ct.IdClient == idClient);
        if (hasTrips)
        {
            return BadRequest("Client has associated trips and cannot be deleted.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, ClientDto clientDto)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Pesel == clientDto.Pesel);
        if (clientExists)
        {
            return BadRequest("Client with this PESEL already exists.");
        }

        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip == null || trip.DateFrom <= DateTime.Now)
        {
            return BadRequest("The trip does not exist or has already taken place.");
        }

        var client = new Client
        {
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Email = clientDto.Email,
            Telephone = clientDto.Telephone,
            Pesel = clientDto.Pesel
        };

        var clientTrip = new ClientTrip
        {
            IdClientNavigation = client,
            IdTripNavigation = trip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientDto.PaymentDate
        };

        _context.Clients.Add(client);
        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(AssignClientToTrip), new { id = client.IdClient }, client);
    }
}