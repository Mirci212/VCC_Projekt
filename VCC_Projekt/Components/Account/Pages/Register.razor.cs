﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace VCC_Projekt.Components.Account.Pages
{

    public partial class Register
    {
        private IEnumerable<IdentityError>? identityErrors;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private string? Message => identityErrors is null ? null : $"Fehler: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        public async Task RegisterUser(EditContext editContext)
        {
            IdentityResult result = new();
            ApplicationUser user = new();
            try
            {
                user = CreateUser();
                user.Firstname = Input.Firstname;
                user.Lastname = Input.Lastname;
                await UserStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                var emailStore = GetEmailStore();
                await emailStore.SetEmailAsync(user, Input.Email.ToLower(), CancellationToken.None);
                result = await UserManager.CreateAsync(user, Input.Password);
            }
            catch (Exception ex)
            {
                result = IdentityResult.Failed(new IdentityError
                {
                    Description = ex.Message
                });
            }

            if (!result.Succeeded)
            {
                identityErrors = result.Errors;
                return;
            }

            Logger.LogInformation("User created a new account with password.");


            // Gibt den Benutzer automatisch die Rolle
            result = await UserManager.AddToRoleAsync(user, "Benutzer");
            if (result.Succeeded)
            {
                Logger.LogInformation($"Benutzer {user.Id} wurde erfolgreich der Rolle 'Benutzer' zugewiesen.");
            }


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
            [Required(ErrorMessage = "E-Mail ist erforderlich.")]
            [EmailAddress]
            [RegularExpression(@"^[a-zA-Z0-9._-]+@[Hh][Tt][Ll][Vv][Bb]\.[Aa][Tt]$",
                ErrorMessage = "Bitte geben Sie eine gültige @htlvb.at " +
                "E-Mail-Adresse ein. Erlaubte Sonderzeichen sind ( . - _ )")]
            [UniqueEmail(ErrorMessage = "Diese Email existiert bereits")]
            [Display(Name = "E-Mail")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Vorname ist erforderlich.")]
            [DataType(DataType.Text)]
            [Display(Name = "Vorname")]
            public string Firstname { get; set; } = "";

            [Required(ErrorMessage = "Nachname ist erforderlich.")]
            [DataType(DataType.Text)]
            [Display(Name = "Nachname")]
            public string Lastname { get; set; } = "";


            [StringLength(100, ErrorMessage = "Der Benutzername muss mind. {2} Zeichen lang sein.", MinimumLength = 3)]
            [RegularExpression(@"^(?!.*@[Hh][Tt][Ll][Vv][Bb]\.[Aa][Tt]).*$",
                ErrorMessage = "Die Schul-Domain '@htlvb.at' ist im Benutzernamen nicht erlaubt.")]
            [DataType(DataType.Text)]
            [Display(Name = "Benutzername")]
            [Required(ErrorMessage = "Benutzername ist erforderlich.")]
            [UniqueUsername(ErrorMessage = "Benutzername existiert bereits")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "Passwort ist erforderlich.")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
                ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein und Groß-/Kleinbuchstaben, Zahlen sowie Sonderzeichen enthalten.")]
            [Display(Name = "Passwort")]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Bestätigungspasswort ist erforderlich.")]
            [DataType(DataType.Password)]
            [Display(Name = "Passwort bestätigen")]
            [Compare("Password", ErrorMessage = "Das Passwort und das Bestätigungspasswort stimmen nicht überein.")]
            public string ConfirmPassword { get; set; } = "";
        }

        /// <summary>
        /// Custom Validations for UniqueUsername
        /// </summary>
        public class UniqueUsernameAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var context = validationContext.GetService<ApplicationDbContext>();
                if (context.ApplicationUsers.Any(u => u.NormalizedUserName == value.ToString().ToUpper()))
                {
                    return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
                }
                return ValidationResult.Success;
            }
        }

        /// <summary>
        /// Custom Validations for UniqueEmail
        /// </summary>
        public class UniqueEmailAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var context = validationContext.GetService<ApplicationDbContext>();
                if (context.ApplicationUsers.Any(u => u.NormalizedEmail == value.ToString().ToUpper()))
                {
                    return new ValidationResult(ErrorMessage, memberNames: [validationContext.MemberName]);
                }
                return ValidationResult.Success;
            }
        }
    }
}
