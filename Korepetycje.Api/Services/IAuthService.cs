using Korepetycje.Api.DTO;

namespace Korepetycje.Api.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterRequestDto request);
        Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginRequestDto request);
    }
}
