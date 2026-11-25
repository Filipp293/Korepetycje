using Korepetycje.Api.Data;
using Korepetycje.Api.DTO;
using Korepetycje.Api.Entities;
using Korepetycje.Api.Services;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace Korepetycje.Api.Services;

public class MongoAuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<ActivationToken> _activationTokens;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public MongoAuthService(MongoDbContext dbContext, IEmailService emailService, IConfiguration configuration)
    {
        _users = dbContext.GetCollection<User>("users");
        _activationTokens = dbContext.GetCollection<ActivationToken>("activation_tokens");
        _emailService = emailService;
        _configuration = configuration;
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

        // Generowanie bezpiecznego losowego tokenu
        string rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        // Hashowanie tokenu przed zapisem w bazie
        string tokenHash = HashToken(rawToken);

        var activationToken = new ActivationToken
        {
            UserId = newUser.Id!,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _activationTokens.InsertOneAsync(activationToken);

        // Wysyłanie e-maila z surowym tokenem w linku
        string clientBaseUrl = _configuration["ClientBaseUrl"] ?? "https://localhost:7164";
        string activationLink = $"{clientBaseUrl}/ConfirmEmail?token={rawToken}";

        await _emailService.SendActivationEmailAsync(newUser.Email, activationLink);

        var response = new AuthResponseDto
        {
            Id = newUser.Id!,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Role = newUser.Role,
            Token = $"mock-jwt-token-{newUser.Id}"
        };

        return (true, "Rejestracja przebiegła pomyślnie. Sprawdź e-mail w celu aktywacji konta.", response);
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
            Token = $"mock-jwt-token-{user.Id}",
            EmailVerified = user.EmailVerified
        };

        return (true, "Zalogowano pomyślnie.", response);
    }

    public async Task<(bool Success, string Message)> ConfirmEmailAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return (false, "Nieprawidłowy token aktywacyjny.");
        }

        // 1. Obliczamy hash przekazanego tokenu
        string tokenHash = HashToken(rawToken);

        // 2. Szukamy niewykorzystanego i niewygasłego tokenu w bazie
        var tokenRecord = await _activationTokens.Find(t =>
            t.TokenHash == tokenHash &&
            t.UsedAt == null &&
            t.ExpiresAt > DateTime.UtcNow
        ).FirstOrDefaultAsync();

        if (tokenRecord == null)
        {
            return (false, "Token jest nieprawidłowy, został już użyty lub wygasł.");
        }

        // 3. Ustawienie users.emailVerified na true
        var userUpdate = Builders<User>.Filter.Eq(u => u.Id, tokenRecord.UserId);
        var updateDef = Builders<User>.Update.Set(u => u.EmailVerified, true);
        await _users.UpdateOneAsync(userUpdate, updateDef);

        // 4. Oznaczenie tokenu jako wykorzystanego (usedAt)
        var tokenUpdate = Builders<ActivationToken>.Filter.Eq(t => t.Id, tokenRecord.Id);
        var tokenUpdateDef = Builders<ActivationToken>.Update.Set(t => t.UsedAt, DateTime.UtcNow);
        await _activationTokens.UpdateOneAsync(tokenUpdate, tokenUpdateDef);

        return (true, "Konto zostało pomyślnie aktywowane!");
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}