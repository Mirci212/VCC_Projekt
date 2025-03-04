﻿using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace VCC_Projekt.Components.Account
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly MailOptions _options;

        public EmailSender(IOptions<MailOptions> mailOptions)
        {
            _options = mailOptions.Value;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            MailMessage message = new MailMessage
            {
                From = new MailAddress(_options.Email),
                Subject = "Bestätige deine E-Mail-Adresse",
                Body = @"
                    <html>
                    <head>
                        <meta charset='utf-8' />
                        <style>
                            body {
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                margin: 0;
                                padding: 20px;
                            }
                            .container {
                                max-width: 600px;
                                margin: auto;
                                background: white;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                            }
                            h1 {
                                color: #333;
                            }
                            p {
                                font-size: 16px;
                                line-height: 1.5;
                                color: #555;
                            }
                            a.button {
                                display: inline-block;
                                padding: 10px 20px;
                                margin: 20px 0;
                                background-color: #007BFF;
                                color: white;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                            }
                            a.button:hover {
                                background-color: #0056b3;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Willkommen!</h1>
                            <p>Vielen Dank, dass du dich bei uns registriert hast. Um deine E-Mail-Adresse zu bestätigen, klicke bitte auf den folgenden Button:</p>
                            <a href='" + confirmationLink + @"' class='button'>E-Mail-Adresse bestätigen</a>
                            <p>Wenn du diese Anfrage nicht gestellt hast, kannst du diese E-Mail ignorieren.</p>
                            <p>Mit freundlichen Grüßen,<br>Dein VCC-Team</p>
                        </div>
                    </body>
                    </html>",
                IsBodyHtml = true,
            }; message.To.Add(email);

            using var smtpClient = new SmtpClient();
            smtpClient.Host = _options.Host;
            smtpClient.Port = _options.Port;
            smtpClient.Credentials = new NetworkCredential(
                _options.Email, _options.Password);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Email: {ex.Message}");
            }
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            MailMessage message = new MailMessage
            {
                From = new MailAddress(_options.Email),
                Subject = "Setze dein Passwort zurück",
                Body = @"
                    <html>
                    <head>
                        <meta charset='utf-8' />
                        <style>
                            body {
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                margin: 0;
                                padding: 20px;
                            }
                            .container {
                                max-width: 600px;
                                margin: auto;
                                background: white;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                            }
                            h1 {
                                color: #333;
                            }
                            p {
                                font-size: 16px;
                                line-height: 1.5;
                                color: #555;
                            }
                            a.button {
                                display: inline-block;
                                padding: 10px 20px;
                                margin: 20px 0;
                                background-color: #007BFF;
                                color: white;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                            }
                            a.button:hover {
                                background-color: #0056b3;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Passwort zurücksetzen!</h1>
                            <p>Um dein Passwort zurückzusetzen, klicke bitte auf den folgenden Button:</p>
                            <a href='" + resetLink + @"' class='button'>Passwort zurücksetzen</a>
                            <p>Wenn du diese Anfrage nicht gestellt hast, kannst du diese E-Mail ignorieren.</p>
                            <p>Mit freundlichen Grüßen,<br>Dein VCC-Team</p>
                        </div>
                    </body>
                    </html>",
                IsBodyHtml = true,
            };

            message.To.Add(email);

            using var smtpClient = new SmtpClient();
            smtpClient.Host = _options.Host;
            smtpClient.Port = _options.Port;
            smtpClient.Credentials = new NetworkCredential(
                _options.Email, _options.Password);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Email: {ex.Message}");
            }

        }

        public async Task SendInvitationLinkAsync(string groupManagerUsername, string groupManagerEmail, string email, string teamName, string invitationLink, string registerLink)
        {
            MailMessage message = new MailMessage
            {
                From = new MailAddress(_options.Email),
                Subject = "Gruppeneinladung",
                Body = @"
                        <html>
                        <head>
                            <meta charset='utf-8' />
                            <style>
                                body {
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    margin: 0;
                                    padding: 20px;
                                }
                                .container {
                                    max-width: 600px;
                                    margin: auto;
                                    background: white;
                                    padding: 20px;
                                    border-radius: 8px;
                                    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                                }
                                h1 {
                                    color: #333;
                                }
                                p {
                                    font-size: 16px;
                                    line-height: 1.5;
                                    color: #555;
                                }
                                a.button {
                                    display: inline-block;
                                    padding: 10px 20px;
                                    margin: 20px 0;
                                    background-color: #007BFF;
                                    color: white;
                                    text-decoration: none;
                                    border-radius: 5px;
                                    font-weight: bold;
                                }
                                a.button:hover {
                                    background-color: #0056b3;
                                }
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>Gruppeneinladung!</h1>
                                <p>Du wurdest von <strong>" + groupManagerUsername + @"</strong> (" + groupManagerEmail + @") eingeladen, um der Gruppe <strong>" + teamName + @"</strong> beizutreten.</p>                                " + (string.IsNullOrEmpty(registerLink) ? string.Empty :
                                "<p>Da du noch nicht registriert bist, klicke bitte auf den folgenden Button, um dich zu registrieren (erst wenn du registriert bist, kannst du einer Gruppe betreten):</p>" +
                                "<a href='" + registerLink + @"' class='button'>Registrieren</a>") + @"
                                <p>Um der Gruppe beizutreten, klicke bitte auf den folgenden Button:</p>
                                <a href='" + invitationLink + @"' class='button'>Gruppe beitreten</a>
                                <p>Wenn du dieser Gruppe nicht betreten willst, kannst du diese E-Mail ignorieren.</p>
                                <p>Mit freundlichen Grüßen,<br>Dein VCC-Team</p>
                            </div>
                        </body>
                        </html>",
                IsBodyHtml = true,
            };

            message.To.Add(email);

            using var smtpClient = new SmtpClient
            {
                Host = _options.Host,
                Port = _options.Port,
                Credentials = new NetworkCredential(_options.Email, _options.Password),
                EnableSsl = true
            };

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Email: {ex.Message}");
            }
        }
    }
}
