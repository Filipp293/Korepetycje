namespace Korepetycje.Api.DTO
{
    public class AuthResponseDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Role { get; set; }
        public required string Token { get; set; }
    }
}
