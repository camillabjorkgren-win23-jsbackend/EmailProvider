using Azure.Messaging.ServiceBus;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace EmailProvider.Functions;

public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IEmailService _emailService;
    private readonly string _queueName;
    private readonly string _serviceBusConnection;
    public EmailSender(ILogger<EmailSender> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
        _queueName = Environment.GetEnvironmentVariable("ServiceBusQueueName")!;
        _serviceBusConnection = Environment.GetEnvironmentVariable("ServiceBusConnection")!;
    }
    [ServiceBusOutput("verification", Connection = "ServiceBusConnection")]
    [Function(nameof(EmailSender))]
    public async Task Run(
       [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var emailRequest = _emailService.UnpackEmailRequest(message);
            if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
            {
                if (_emailService.SendEmail(emailRequest))
                {
                    await messageActions.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message}");
        }
    }
}

