using Korepetycje.Api.Data;
using Korepetycje.Api.DTO;
using Korepetycje.Api.Entities;
using Korepetycje.Api.Services;
using MongoDB.Driver;

namespace MojaAplikacja.Api.Services;

public class MongoAuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;

    public MongoAuthService(MongoDbContext dbContext)
    {
        _users = dbContext.GetCollection<User>("users");
    }

    // Implementacja metody rejestracji użytkownika
    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterRequestDto request)
    {
        // Sprawdzanie czy użytkownik z takim mailem już istnieje
        var existingUser = await _users.Find(u => u.Email.ToLower() == request.Email.ToLower()).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return (false, "Użytkownik o podanym adresie email już istnieje.", null);
        }

        // Mapowanie rodzaju konta z Blazora (0 -> tutor, 1 -> parent)
        string role = request.RodzajKonta == 0 ? "tutor" : "parent";

        // Hashowanie hasła przy użyciu BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            FirstName = request.Name,
            LastName = request.LastName,
            Role = role,
            EmailVerified = false
        };

        // Zapis w bazie danych MongoDB
        await _users.InsertOneAsync(newUser);

        var response = new AuthResponseDto
        {
            Id = newUser.Id!,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Role = newUser.Role,
            Token = $"mock-jwt-token-{newUser.Id}"
        };

        return (true, "Rejestracja przebiegła pomyślnie.", response);
    }

    // Implementacja metody logowania użytkownika
    public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginRequestDto request)
    {
        // Szukamy użytkownika po emailu
        var user = await _users.Find(u => u.Email.ToLower() == request.Email.ToLower()).FirstOrDefaultAsync();
        if (user == null)
        {
            return (false, "Nieprawidłowy e-mail lub hasło.", null);
        }

        // Weryfikujemy hash hasła
        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch
        {
            isPasswordValid = false;
        }

        // Jeśli hasło jest nieprawidłowe, zwracamy odpowiedni komunikat
        if (!isPasswordValid)
        {
            return (false, "Nieprawidłowy e-mail lub hasło.", null);
        }

        // Tworzymy odpowiedź z danymi użytkownika
        var response = new AuthResponseDto
        {
            Id = user.Id!,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = $"mock-jwt-token-{user.Id}"
        };

        return (true, "Zalogowano pomyślnie.", response);
    }
}