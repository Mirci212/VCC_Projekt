using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VCC_Projekt.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;

    public class ApplicationUser : IdentityUser<string>
    {
        // Der Benutzername wird als ID verwendet
        public override string Id
        {
            get => UserName;
            set => UserName = value;
        }
    }






}