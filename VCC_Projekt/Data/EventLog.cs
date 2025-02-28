namespace VCC_Projekt.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations.Schema;

public class EventLog
{
    public EventLog()
    {
        
    }

    public EventLog(string tabellenname, string beschreibung, int logKategorie_KatID)
    {
        Tabellenname = tabellenname;
        Beschreibung = beschreibung;
        LogKategorie_KatID = logKategorie_KatID;
    }

    public int EventLogID { get; set; }

    public string Tabellenname { get; set; }

    public string Beschreibung { get; set; }

    public DateTime Zeit { get; set; }

    // Fremdschlüssel zu Level
    public int LogKategorie_KatID { get; set; }

    // Navigation zu Level
    [ForeignKey("LogKategorie_KatID")]
    public virtual LogKat LogKat { get; set; }
}
public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("vcc_eventlog");
        builder.HasKey(t => t.EventLogID);

        // Korrekte Beziehung zu Level
        builder.HasOne(t => t.LogKat)
              .WithMany(t => t.EventLogs)
              .HasForeignKey(t => t.LogKategorie_KatID)
              .HasPrincipalKey(l => l.KatID);
    }
}

