namespace Korepetycje.Api.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        // Metoda do wysyłania wiadomości e-mail z linkiem aktywacyjnym
        public Task SendActivationEmailAsync(string toEmail, string activationLink)
        {
            _logger.LogInformation("\n=========================================");
            _logger.LogInformation("EMAIL DO: {To}", toEmail);
            _logger.LogInformation("LINK AKTYWACYJNY: {Link}", activationLink);
            _logger.LogInformation("=========================================\n");

            return Task.CompletedTask;
        }
    }
}
