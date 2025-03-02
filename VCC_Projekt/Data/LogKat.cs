namespace VCC_Projekt.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations.Schema;
public class LogKat
{
    public LogKat()
    {

    }

    public LogKat(string beschreibung)
    {
        Beschreibung = beschreibung;
    }

    public int KatID { get; set; }

    public string Beschreibung { get; set; }

    public List<EventLog> EventLogs { get; set; }
}

public class LogKatConfiguration : IEntityTypeConfiguration<LogKat>
{
    public void Configure(EntityTypeBuilder<LogKat> builder)
    {
        builder.ToTable("vcc_logkategorie");
        builder.HasKey(t => t.KatID);

        // Korrekte Beziehung zu Level
        builder.HasMany(t => t.EventLogs)
              .WithOne(t => t.LogKat)
              .HasForeignKey(t => t.LogKategorie_KatID)
              .HasPrincipalKey(l => l.KatID);
    }
}