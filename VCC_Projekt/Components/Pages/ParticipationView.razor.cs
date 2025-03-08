using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MySqlConnector;
using System.Security.Claims;

namespace VCC_Projekt.Components.Pages
{
    public partial class ParticipationView
    {
        static string Dashboardlink = "/Dashboard/";

        [Parameter]
        public int EventId { get; set; }

        private Event? Event { get; set; } = null;
        private Level? CurrentLevel;
        private Dictionary<int, UploadedFile> UploadedFiles { get; set; } = new();
        private ClaimsPrincipal User { get; set; }
        private Gruppe? Group { get; set; }

        private bool AllFilesSubmitted { get; set; } = false;
        private bool isLoading = false;
        private bool accessDenied = false;
        private string accessDeniedMessage = "";
        private int Fehlversuche;
        private int Platzeriung;
        private List<RanglisteResult> Rangliste { get; set; } = new();

        protected override async void OnInitialized()
        {
            isLoading = true;
            try
            {
                // Lade das Event und die Gruppe
                Event = dbContext.Events
                    .Where(e => e.EventID == EventId)
                    .Select(e => new Event
                    {
                        EventID = e.EventID,
                        Bezeichnung = e.Bezeichnung,
                        Beginn = e.Beginn,
                        Dauer = e.Dauer,
                        StrafminutenProFehlversuch = e.StrafminutenProFehlversuch,
                        Levels = e.Levels.Select(l => new Level
                        {
                            LevelID = l.LevelID,
                            Levelnr = l.Levelnr
                        }).OrderBy(le => le.Levelnr).ToList()
                    })
                    .FirstOrDefault();

                if (Event == null) throw new ArgumentException("Event nicht gefunden");

                // Überprüfe, ob das Event aktuell läuft
                DateTime now = DateTime.Now;
                if (now < Event.Beginn)
                {
                    throw new ArgumentException("Das Event hat noch nicht begonnen.");
                }
                if (now > Event.Beginn.AddMinutes(Event.Dauer))
                {
                    throw new ArgumentException("Das Event ist bereits beendet.");
                }

                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                User = authState.User;

                if (User.Identity == null || !User.Identity.IsAuthenticated)
                    throw new ArgumentException("Benutzer nicht gefunden");

                Group = dbContext.Gruppen
                    .Where(gr => gr.Event_EventID == EventId && gr.UserInGruppe.Any(us => us.User_UserId == User.Identity.Name))
                    .Include(g => g.Absolviert)
                    .Include(n => n.UserInGruppe)
                    .FirstOrDefault();



                if (Group == null) throw new ArgumentException("Gruppe nicht gefunden");
                if (Group.Gesperrt) throw new ArgumentException("Gruppe gesperrt");

                // Lade die Rangliste mit der gespeicherten Prozedur
                Rangliste = dbContext.Set<RanglisteResult>().FromSqlRaw("CALL ShowRangliste(@eventId)", new MySqlParameter("@eventId", EventId)).ToList();

                // Berechne die Platzierung der aktuellen Gruppe
                var gruppeRang = Rangliste.FirstOrDefault(r => r.GruppenID == Group.GruppenID);
                Platzeriung = gruppeRang?.Rang ?? 0;

                // Lade das aktuelle Level
                CurrentLevel = dbContext.Levels
                    .Where(level => level.Event_EventID == EventId)
                    .Where(level => !dbContext.GruppeAbsolviertLevels
                        .Any(a => a.Level_LevelID == level.LevelID && a.Gruppe_GruppeID == Group.GruppenID && a.BenoetigteZeit != null))
                    .OrderBy(l => l.Levelnr)
                    .Select(l => new Level
                    {
                        LevelID = l.LevelID,
                        Levelnr = l.Levelnr,
                        Aufgaben = l.Aufgaben.Select(au => new Aufgabe
                        {
                            AufgabenID = au.AufgabenID,
                            Aufgabennr = au.Aufgabennr
                        }).ToList()
                    })
                    .FirstOrDefault();


                if (CurrentLevel == null)
                {
                    if (Platzeriung != 0) throw new ArgumentException("Alle Levels abgeschlossen");
                    else
                    {
                        throw new ArgumentException("Kein Level gefunden");
                    }
                }


                if (CurrentLevel.Aufgaben.Count == 0) AllFilesSubmitted = true;
                Fehlversuche = Group.Absolviert.Where(ab => ab.Level_LevelID == CurrentLevel.LevelID).Select(ab => ab.Fehlversuche).FirstOrDefault();
            }
            catch (Exception ex)
            {
                accessDenied = true;
                accessDeniedMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JS.InvokeVoidAsync("startTimer", Event.Dauer);
                }
                catch (Exception) { }
            }
        }

        private async Task UploadFile(IBrowserFile file, int aufgabenId)
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var uploadedFile = new UploadedFile(file.Name, memoryStream.ToArray());

            // Falls bereits eine Datei existiert, wird sie überschrieben
            UploadedFiles[aufgabenId] = uploadedFile;
        }

        private async Task SubmitFile(Aufgabe aufgabe)
        {
            if (Event.Beginn.AddMinutes((double)Event.Dauer) < DateTime.Now)
            {
                OnInitialized();
                return;
            }
            if (UploadedFiles.TryGetValue(aufgabe.AufgabenID, out var uploadedFile))
            {
                var ergebnisTxt = await dbContext.Aufgabe
                                                .Where(au => au.AufgabenID == aufgabe.AufgabenID)
                                                .Select(au => au.Ergebnis_TXT)
                                                .FirstOrDefaultAsync();
                if (ergebnisTxt == null) return;

                string uploadedContent = System.Text.Encoding.UTF8.GetString(uploadedFile.FileData).Trim();
                string correctContent = System.Text.Encoding.UTF8.GetString(ergebnisTxt).Trim();

                bool isCorrect = uploadedContent == correctContent;
                if (!isCorrect && uploadedFile.FileIsRight == null)
                {
                    var absolviertLevel = await dbContext.GruppeAbsolviertLevels
                                                        .FirstOrDefaultAsync(a => a.Gruppe_GruppeID == Group.GruppenID && a.Level_LevelID == CurrentLevel.LevelID);
                    if (absolviertLevel != null)
                    {
                        absolviertLevel.Fehlversuche++;
                        Fehlversuche = absolviertLevel.Fehlversuche;
                        StateHasChanged();
                    }
                    else
                    {
                        var newAbsolviertLevel = new GruppeAbsolviertLevel
                        {
                            Gruppe_GruppeID = Group.GruppenID,
                            Level_LevelID = CurrentLevel.LevelID,
                            Fehlversuche = 1,
                            BenoetigteZeit = null
                        };
                        dbContext.GruppeAbsolviertLevels.Add(newAbsolviertLevel);
                        Fehlversuche = 1;
                    }
                    dbContext.SaveChanges();
                }
                UploadedFiles[aufgabe.AufgabenID] = uploadedFile with { FileIsRight = isCorrect };
            }



            // Prüfen, ob alle Aufgaben eine richtige Datei haben
            if (CurrentLevel?.Aufgaben.Count == 0)
            {
                AllFilesSubmitted = true;
                return;
            }
            AllFilesSubmitted = CurrentLevel?.Aufgaben.All(a =>
                UploadedFiles.ContainsKey(a.AufgabenID) &&
                UploadedFiles[a.AufgabenID].FileIsRight == true) ?? false;
        }
        private async Task ProceedToNextLevel()
        {
            TimeSpan benoetigteZeit = DateTime.Now - Event.Beginn;
            if (benoetigteZeit > TimeSpan.FromMinutes((double)Event.Dauer))
            {
                OnInitialized();
                return;
            }
            var absolviert = await dbContext.GruppeAbsolviertLevels
                                            .FirstOrDefaultAsync(g => g.Gruppe_GruppeID == Group.GruppenID
                                                                && g.Level_LevelID == CurrentLevel.LevelID);
            if (absolviert == null)
            {
                // Neuen Datensatz erstellen
                absolviert = new GruppeAbsolviertLevel
                {
                    Gruppe_GruppeID = Group.GruppenID,
                    Level_LevelID = CurrentLevel.LevelID,
                    Fehlversuche = Fehlversuche
                };

                dbContext.GruppeAbsolviertLevels.Add(absolviert);
            }
            absolviert.BenoetigteZeit = benoetigteZeit;

            await dbContext.SaveChangesAsync();
            AllFilesSubmitted = false;
            OnInitialized();
        }
        public record UploadedFile(string FileName, byte[] FileData, bool? FileIsRight = null);
    }

}