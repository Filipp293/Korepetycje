namespace Korepetycje.Api.DTO
{
    public class RegisterRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public int RodzajKonta { get; set; }
    }
}
