using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace DutyScheduler.Services
{

    public class EmailDeliveryException : Exception
    {

    }

    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender
    {
        private readonly SmtpClientConfiguration _smtpClientConfiguration;

        /// <summary>
        /// Could be used for details e-mail errors logging.
        /// </summary>
        //private readonly ILogger _logger;

        public AuthMessageSender(
            IOptions<SmtpClientConfiguration> smtpConfigurationAccessor)
        {
            _smtpClientConfiguration = smtpConfigurationAccessor.Value;
            //_logger = logger;

        }

        public async Task SendEmailAsync(string to, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_smtpClientConfiguration.MailboxName, _smtpClientConfiguration.MailboxAddress));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                //client.LocalDomain = "";

                // Sending the e-mail
                try
                {

                    // Try to connect to the SMTP server.
                    try
                    {
                        await client.ConnectAsync(_smtpClientConfiguration.Host, _smtpClientConfiguration.Port, SecureSocketOptions.Auto).ConfigureAwait(true);
                    }
                    catch (SmtpCommandException ex)
                    {
                        //_logger.LogError("Error trying to connect: {0}", ex.Message, ex);
                        Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);
                        return;
                    }
                    catch (SmtpProtocolException)
                    {
                        //_logger.LogError("Protocol error while trying to connect: {0}", ex.Message, ex);
                        return;
                    }

                    // Try to authenticate to the server.
                    try
                    {
                        await client.AuthenticateAsync(_smtpClientConfiguration.Username, _smtpClientConfiguration.Password);
                    }
                    catch (AuthenticationException)
                    {
                        //_logger.LogError("Invalid user name or password.", ex);
                        return;
                    }
                    catch (SmtpCommandException)
                    {
                        //_logger.LogError("Error trying to authenticate: {0}", ex.Message, ex);
                        //_logger.LogError("\tStatusCode: {0}", ex.StatusCode, ex);
                        return;
                    }
                    catch (SmtpProtocolException)
                    {
                        //_logger.LogError("Protocol error while trying to authenticate: {0}", ex.Message, ex);
                        return;
                    }

                    // Try to send the e-mail message.
                    try
                    {
                        await client.SendAsync(emailMessage).ConfigureAwait(false);
                    }
                    catch (SmtpCommandException ex)
                    {
                        //_logger.LogError("Error sending message: {0}", ex.Message, ex);
                        //_logger.LogError("\tStatusCode: {0}", ex.StatusCode, ex);

                        switch (ex.ErrorCode)
                        {
                            case SmtpErrorCode.RecipientNotAccepted:
                                //_logger.LogError("\tRecipient not accepted: {0}", ex.Mailbox, ex);
                                break;
                            case SmtpErrorCode.SenderNotAccepted:
                                //_logger.LogError("\tSender not accepted: {0}", ex.Mailbox, ex);
                                break;
                            case SmtpErrorCode.MessageNotAccepted:
                                //_logger.LogError("\tMessage not accepted.", ex);
                                break;
                        }
                    }
                    catch (SmtpProtocolException)
                    {
                        //_logger.LogError("Protocol error while sending message: {0}", ex.Message, ex);
                    }

                }
                catch (Exception)
                {
                    throw new EmailDeliveryException(); 
                }
                
                await client.DisconnectAsync(true).ConfigureAwait(true);
            }
        }
    }
}
