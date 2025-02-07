﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VCC_Projekt.Data;

#nullable disable

namespace VCC_Projekt.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("vcc_AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("vcc_AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(256)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("vcc_AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(256)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(256)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("vcc_AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(256)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("vcc_AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.ApplicationRole", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("Beschreibung")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Id")
                        .HasColumnType("longtext");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Name");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("vcc_AspNetRoles", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.ApplicationUser", b =>
                {
                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.HasKey("UserName");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("vcc_AspNetUsers", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.Aufgabe", b =>
                {
                    b.Property<int>("AufgabenID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("AufgabenID"));

                    b.Property<int>("Aufgabennr")
                        .HasColumnType("int");

                    b.Property<byte[]>("Ergebnis_TXT")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<byte[]>("Input_TXT")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<int>("Level_LevelID")
                        .HasColumnType("int");

                    b.HasKey("AufgabenID");

                    b.HasIndex("Level_LevelID");

                    b.ToTable("vcc_aufgaben", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.Event", b =>
                {
                    b.Property<int>("EventID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("EventID"));

                    b.Property<DateTime>("Beginn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Bezeichnung")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Dauer")
                        .HasColumnType("int");

                    b.Property<int>("StrafminutenProFehlversuch")
                        .HasColumnType("int");

                    b.HasKey("EventID");

                    b.ToTable("vcc_event", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.Gruppe", b =>
                {
                    b.Property<int>("GruppenID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("GruppenID"));

                    b.Property<int>("Event_EventID")
                        .HasColumnType("int");

                    b.Property<string>("GruppenleiterId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Gruppenname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Teilnehmertyp")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("GruppenID");

                    b.HasIndex("Event_EventID");

                    b.HasIndex("GruppenleiterId");

                    b.ToTable("vcc_gruppe", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.GruppeAbsolviertLevel", b =>
                {
                    b.Property<int>("Gruppe_GruppeID")
                        .HasColumnType("int");

                    b.Property<int>("Level_LevelID")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("BenoetigteZeit")
                        .HasColumnType("time(6)");

                    b.Property<int>("Fehlversuche")
                        .HasColumnType("int");

                    b.HasKey("Gruppe_GruppeID", "Level_LevelID");

                    b.ToTable("vcc_gruppe_absolviert_level", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.Level", b =>
                {
                    b.Property<int>("LevelID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("LevelID"));

                    b.Property<byte[]>("Angabe_PDF")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<int>("Event_EventID")
                        .HasColumnType("int");

                    b.Property<int>("Levelnr")
                        .HasColumnType("int");

                    b.HasKey("LevelID");

                    b.HasIndex("Event_EventID");

                    b.ToTable("vcc_level", (string)null);
                });

            modelBuilder.Entity("VCC_Projekt.Data.UserInGruppe", b =>
                {
                    b.Property<string>("User_UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Gruppe_GruppenId")
                        .HasColumnType("int");

                    b.HasKey("User_UserId", "Gruppe_GruppenId");

                    b.HasIndex("Gruppe_GruppenId");

                    b.ToTable("vcc_UserInGruppe", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("VCC_Projekt.Data.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("VCC_Projekt.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("VCC_Projekt.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("VCC_Projekt.Data.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VCC_Projekt.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("VCC_Projekt.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VCC_Projekt.Data.Aufgabe", b =>
                {
                    b.HasOne("VCC_Projekt.Data.Level", "Level")
                        .WithMany("Aufgaben")
                        .HasForeignKey("Level_LevelID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Level");
                });

            modelBuilder.Entity("VCC_Projekt.Data.Gruppe", b =>
                {
                    b.HasOne("VCC_Projekt.Data.Event", "Event")
                        .WithMany("Gruppen")
                        .HasForeignKey("Event_EventID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("VCC_Projekt.Data.ApplicationUser", "GruppenleiterNavigation")
                        .WithMany("GruppenleiterNavigation")
                        .HasForeignKey("GruppenleiterId")
                        .HasPrincipalKey("Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("GruppenleiterNavigation");
                });

            modelBuilder.Entity("VCC_Projekt.Data.GruppeAbsolviertLevel", b =>
                {
                    b.HasOne("VCC_Projekt.Data.Gruppe", "Gruppe")
                        .WithMany("Absolviert")
                        .HasForeignKey("Gruppe_GruppeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VCC_Projekt.Data.Level", "Level")
                        .WithMany("Absolviert")
                        .HasForeignKey("Gruppe_GruppeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gruppe");

                    b.Navigation("Level");
                });

            modelBuilder.Entity("VCC_Projekt.Data.Level", b =>
                {
                    b.HasOne("VCC_Projekt.Data.Event", "Event")
                        .WithMany("Levels")
                        .HasForeignKey("Event_EventID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("VCC_Projekt.Data.UserInGruppe", b =>
                {
                    b.HasOne("VCC_Projekt.Data.Gruppe", "Gruppe")
                        .WithMany("UserInGruppe")
                        .HasForeignKey("Gruppe_GruppenId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VCC_Projekt.Data.ApplicationUser", "User")
                        .WithMany("UserInGruppe")
                        .HasForeignKey("User_UserId")
                        .HasPrincipalKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gruppe");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VCC_Projekt.Data.ApplicationUser", b =>
                {
                    b.Navigation("GruppenleiterNavigation");

                    b.Navigation("UserInGruppe");
                });

            modelBuilder.Entity("VCC_Projekt.Data.Event", b =>
                {
                    b.Navigation("Gruppen");

                    b.Navigation("Levels");
                });

            modelBuilder.Entity("VCC_Projekt.Data.Gruppe", b =>
                {
                    b.Navigation("Absolviert");

                    b.Navigation("UserInGruppe");
                });

            modelBuilder.Entity("VCC_Projekt.Data.Level", b =>
                {
                    b.Navigation("Absolviert");

                    b.Navigation("Aufgaben");
                });
#pragma warning restore 612, 618
        }
    }
}
