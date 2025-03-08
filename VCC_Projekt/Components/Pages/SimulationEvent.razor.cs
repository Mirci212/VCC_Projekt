using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace VCC_Projekt.Components.Pages
{
    public partial class SimulationEvent
    {
        [Parameter]
        public int EventId { get; set; }

        private Event? Event { get; set; } = null;
        private Level? CurrentLevel;
        private Dictionary<int, UploadedFile> UploadedFiles { get; set; } = new();

        private bool AllFilesSubmitted { get; set; } = false;
        private bool isLoading = false;
        private string accessDeniedMessage = "";
        List<RanglisteResult> currRangliste { get; set; } = new();
        private List<GruppeAbsolviertLevel> AbsolviertLevels { get; set; } = new();
        int currLevelNr = 1;
        int currFehlversuche = 0;
        int gesamtFehlverusche = 0;
        DateTime startTime = DateTime.Now;

        protected override async void OnInitialized()
        {
            isLoading = true;
            try
            {
                Event = dbContext.Events
                    .Where(e => e.EventID == EventId)
                    .Select(e => new Event
                    {
                        EventID = e.EventID,
                        Bezeichnung = e.Bezeichnung,
                        StrafminutenProFehlversuch = e.StrafminutenProFehlversuch,
                        Levels = e.Levels.Select(l => new Level
                        {
                            LevelID = l.LevelID,
                            Levelnr = l.Levelnr
                        }).OrderBy(le => le.Levelnr).ToList()
                    })
                    .FirstOrDefault();

                if (Event == null) throw new ArgumentException("Event nicht gefunden");

                // Lade das aktuelle Level
                CurrentLevel = dbContext.Levels
                    .Where(level => level.Event_EventID == EventId && level.Levelnr == currLevelNr)
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
                    if (currLevelNr != 0) throw new ArgumentException("Alle Levels abgeschlossen");
                    else
                    {
                        throw new ArgumentException("Kein Level gefunden");
                    }
                }


                if (CurrentLevel.Aufgaben.Count == 0) AllFilesSubmitted = true;
            }
            catch (Exception ex)
            {
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
                    await JS.InvokeVoidAsync("startTimer", int.MaxValue);
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
                    currFehlversuche++;
                    gesamtFehlverusche++;
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

            GruppeAbsolviertLevel gruppeAbsolviertLevel = new()
            {
                Gruppe_GruppeID = 0,
                Level_LevelID = CurrentLevel.LevelID,
                Fehlversuche = currFehlversuche,
                BenoetigteZeit = startTime - DateTime.Now
            };

            AbsolviertLevels.Add(gruppeAbsolviertLevel);

            RanglisteResult ranglisteResult = new()
            {
                Rang = 1,
                Gruppenname = "TestUser",
                AnzahlLevel = currLevelNr,
                MaxBenötigteZeit = (DateTime.Now - startTime),
                GesamtFehlversuche = gesamtFehlverusche,
                GebrauchteZeit = (DateTime.Now - startTime).Add(TimeSpan.FromMinutes(gesamtFehlverusche * Event.StrafminutenProFehlversuch))
            };
            currRangliste = new List<RanglisteResult>() { ranglisteResult };

            currLevelNr++;

            currFehlversuche = 0;
            AllFilesSubmitted = false;
            OnInitialized();
        }
        public record UploadedFile(string FileName, byte[] FileData, bool? FileIsRight = null);
    }
}