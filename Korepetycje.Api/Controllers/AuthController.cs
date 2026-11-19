using Korepetycje.Api.DTO;
using Korepetycje.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Korepetycje.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Endpoint do rejestracji użytkownika
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result.Data);
    }

    // Endpoint do logowania użytkownika
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result.Data);
    }
}