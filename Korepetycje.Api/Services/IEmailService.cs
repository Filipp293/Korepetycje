namespace Korepetycje.Api.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string toEmail, string activationLink);
    }
}
