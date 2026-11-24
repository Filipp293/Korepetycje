using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Korepetycje.Api.Data;
using Korepetycje.Api.Entities;

namespace Korepetycje.Api.Controllers;

// Testowy kontroler do testowania połączenia z MongoDB i pobierania danych użytkowników
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MongoDbContext _context;

    public UsersController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var usersCollection = _context.GetCollection<User>("users");
            var users = await usersCollection.Find(_ => true).ToListAsync();

            return Ok(new
            {
                Success = true,
                Message = "Pomyślnie połączono z MongoDB!",
                Count = users.Count,
                Data = users
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Błąd podczas pobierania danych z MongoDB.",
                Error = ex.Message
            });
        }
    }
}