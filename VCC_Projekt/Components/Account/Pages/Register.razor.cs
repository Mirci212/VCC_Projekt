﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using VCC_Projekt.Data;
using System.Text.RegularExpressions;

namespace VCC_Projekt.Components.Account.Pages
{
    public partial class Register
    {
        private IEnumerable<IdentityError>? identityErrors;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        public async Task RegisterUser(EditContext editContext)
        {
            var user = CreateUser();
            
            await UserStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                identityErrors = result.Errors;
                return;
            }

            Logger.LogInformation("User created a new account with password.");

            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (UserManager.Options.SignIn.RequireConfirmedAccount)
            {
                RedirectManager.RedirectTo(
                    "Account/RegisterConfirmation",
                    new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
            }

            await SignInManager.SignInAsync(user, isPersistent: false);
            RedirectManager.RedirectTo(ReturnUrl);
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
        }

        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            [RegularExpression(@"^[a-zA-Z0-9._-]+@[Hh][Tt][Ll][Vv][Bb]\.[Aa][Tt]$", ErrorMessage = "Bitte geben Sie eine gültige @htlvb.at " +
                "E-Mail-Adresse ein. Erlaubte Sonderzeichen sind ( . - _ )")]
            [Display(Name = "E-Mail")]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Firstname")]
            public string Firstname { get; set; } = "";

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Lastname")]
            public string Lastname { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "Der Benutzername muss zwischen {2} und {1} Zeichen lang sein.", MinimumLength = 3)]
            [DataType(DataType.Text)]
            [Display(Name = "Benutzername")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "Das Passwort ist erforderlich.")]
            [DataType(DataType.Password)]
            [RegularExpression( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", 
                ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein und Groß-/Kleinbuchstaben, Zahlen sowie Sonderzeichen enthalten.")]
            [Display(Name = "Passwort")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Passwort Bestätigen")]
            [Compare("Password", ErrorMessage = "Das Passwort und das Bestätigungspasswort stimmen nicht überein.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}
