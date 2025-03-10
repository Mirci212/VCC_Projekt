using System.ComponentModel.DataAnnotations.Schema;

namespace VCC_Projekt.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationUser : IdentityUser<string>
{
    public ApplicationUser()
    {
    }

    public ApplicationUser(string firstname, string lastname)
    {
        Firstname = firstname;
        Lastname = lastname;
    }

    // Der Benutzername wird als ID verwendet
    public override string Id
    {
        get => UserName;
        set => UserName = value;
    }
    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public List<Gruppe>? GruppenleiterNavigation { get; set; }

    public List<UserInGruppe>? UserInGruppe { get; set; }

    public string Fullname => $"{Firstname} {Lastname}";
}

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("vcc_AspNetUsers");
        builder.HasKey(u => u.UserName);
        builder.Property(u => u.UserName)
                  .IsRequired()
                  .HasMaxLength(256);

        builder.HasMany(u => u.UserInGruppe)
           .WithOne(g => g.User)           
           .HasForeignKey(g => g.User_UserId)   
           .HasPrincipalKey(u => u.UserName)    
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.GruppenleiterNavigation)
          .WithOne(g => g.GruppenleiterNavigation)
          .HasForeignKey(g => g.GruppenleiterId)
          .HasPrincipalKey(u => u.UserName)
          .OnDelete(DeleteBehavior.Restrict);

        builder.Property(u => u.NormalizedUserName)
              .HasMaxLength(256);
        builder.Ignore(e => e.LockoutEnabled);
        builder.Ignore(e => e.LockoutEnd);
        builder.Ignore(e => e.PhoneNumber);
        builder.Ignore(e => e.PhoneNumberConfirmed);
        builder.Ignore(e => e.TwoFactorEnabled);
        builder.Ignore(e => e.Id);
        builder.Ignore("GruppenId");
    }
}