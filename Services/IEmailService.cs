using Azure.Messaging.ServiceBus;
using EmailProvider.Models;

namespace EmailProvider.Services;
public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailRequest emailRequest);
    EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message);
}