using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace VCC_Projekt.Components.Pages
{
    public partial class AddLevels
    {
        private List<LevelViewModel> _levels = new();
        private List<Event> _events = new();
        private Event _selectedEvent = new() { EventID = 0 };
        private bool IsEventInPast => _events.FirstOrDefault(e => e.EventID == _selectedEvent.EventID)?.Beginn < DateTime.Now;

        protected override void OnInitialized()
        {
            _events = dbContext.Events.OrderByDescending(ev => ev.Beginn).ToList();
        }

        private async Task OnEventSelected(Event selectedEvent)
        {
            _selectedEvent = selectedEvent;
            _levels = _selectedEvent.EventID != 0
                ? await LoadLevelsAsync()
                : new List<LevelViewModel>();
        }

        private async Task<List<LevelViewModel>> LoadLevelsAsync() =>
            await dbContext.Levels
                .Where(le => le.Event_EventID == _selectedEvent.EventID)
                .Select(level => new LevelViewModel
                {
                    LevelId = level.LevelID,
                    Levelnr = level.Levelnr,
                    Aufgaben = level.Aufgaben.Select(aufgabe => new AufgabeViewModel
                    {
                        AufgabenId = aufgabe.AufgabenID,
                        Aufgabennr = aufgabe.Aufgabennr,
                        IsExpanded = false
                    }).ToList(),
                    IsExpanded = false
                })
                .ToListAsync();

        private void AddLevel()
        {
            if (CanAddLevel())
            {
                _levels.Add(new LevelViewModel
                {
                    Levelnr = _levels.Count + 1,
                    Aufgaben = new List<AufgabeViewModel>(),
                    IsExpanded = false
                });
            }
        }

        private bool CanAddLevel() => _levels.Count < 5 && _selectedEvent.EventID != 0 && !IsEventInPast;

        private void RemoveLevel(int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            if (level != null && CanRemoveLevel(levelnr))
            {
                _levels.Remove(level);
                if (!level.IsNew)
                {
                    dbContext.Levels.Remove(dbContext.Levels.Find(level.LevelId));
                }
                ReorderLevels();
            }
        }

        private bool CanRemoveLevel(int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            return level != null && !IsEventInPast;
        }

        private void AddTask(int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            if (level != null && CanModifyTask(level.Levelnr))
            {
                level.Aufgaben.Add(new AufgabeViewModel
                {
                    Aufgabennr = level.Aufgaben.Count + 1,
                    IsExpanded = false
                });
            }
        }

        private void RemoveTask(int levelnr, int aufgabennr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            if (level != null && CanModifyTask(level.Levelnr))
            {
                var task = level.Aufgaben.FirstOrDefault(a => a.Aufgabennr == aufgabennr);
                if (task != null)
                {
                    level.Aufgaben.Remove(task);
                    if (!task.IsNew)
                    {
                        dbContext.Aufgabe
                            .Where(a => a.AufgabenID == task.AufgabenId)
                            .ExecuteDelete();
                    }
                }
            }
        }

        private bool CanModifyTask(int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            return level != null && !IsEventInPast;
        }

        private void ToggleLevel(int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            if (level != null) level.IsExpanded = !level.IsExpanded;
        }

        private void ToggleTask(int levelnr, int aufgabennr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            var task = level?.Aufgaben.FirstOrDefault(a => a.Aufgabennr == aufgabennr);
            if (task != null) task.IsExpanded = !task.IsExpanded;
        }

        private async Task UploadFile(IBrowserFile file, int levelnr)
        {
            if (CanUploadFile(file, levelnr))
            {
                var level = _levels.First(l => l.Levelnr == levelnr);
                var fileBytes = await ConvertToBytesAsync(file);

                level.NewAngabe_PDF = fileBytes;
                level.AngabeUpdated = true;

                ShowSnackbar("PDF erfolgreich hochgeladen!", Severity.Success);
            }
            else
            {
                ShowSnackbar("Fehler beim Hochladen des PDFs! (max Size: 4MB)", Severity.Error);
            }
        }

        private async Task UploadTaskFile(IBrowserFile file, int levelnr, int aufgabennr, string type)
        {
            if (CanUploadTaskFile(file, levelnr, aufgabennr))
            {
                var level = _levels.First(l => l.Levelnr == levelnr);
                var task = level.Aufgaben.First(a => a.Aufgabennr == aufgabennr);
                var fileBytes = await ConvertToBytesAsync(file);

                if (type == "input")
                {
                    task.NewInput_TXT = fileBytes;
                    task.InputUpdated = true;
                }
                else
                {
                    task.NewErgebnis_TXT = fileBytes;
                    task.ErgebnisUpdated = true;
                }

                ShowSnackbar("Datei erfolgreich hochgeladen!", Severity.Success);
            }
            else
            {
                ShowSnackbar("Fehler beim Hochladen der Datei! (max Size. 4MB)", Severity.Error);
            }
        }

        private bool CanUploadFile(IBrowserFile file, int levelnr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            return level != null
                && !IsEventInPast
                && file != null
                && file.ContentType == "application/pdf"
                && file.Size <= 4 * 1024 * 1024;
        }

        private bool CanUploadTaskFile(IBrowserFile file, int levelnr, int aufgabennr)
        {
            var level = _levels.FirstOrDefault(l => l.Levelnr == levelnr);
            var task = level?.Aufgaben.FirstOrDefault(a => a.Aufgabennr == aufgabennr);
            return task != null
                && !IsEventInPast
                && file != null
                && file.ContentType == "text/plain"
                && file.Size <= 4 * 1024 * 1024;
        }

        private async Task SaveLevels()
        {
            try
            {
                _levels.ForEach(l => { l.IsExpanded = false; l.Aufgaben.ForEach(a => a.IsExpanded = false); });
                ValidateBeforeSave();
                ShowSnackbar("Levels werden gespeichert...", Severity.Info);
                await ProcessDatabaseOperations();
                await dbContext.SaveChangesAsync();

                _levels = await LoadLevelsAsync();
                Snackbar.Remove(Snackbar.ShownSnackbars.Where(sn => sn.Message == "Levels werden gespeichert...").FirstOrDefault());
                ShowSnackbar("Levels erfolgreich gespeichert!", Severity.Success);
            }
            catch (Exception ex)
            {
                if (ex.Source == "Microsoft.EntityFrameworkCore.Relational" || ex.Message.StartsWith("A second operation")) ShowSnackbar($"Fehler beim Speichern des Levels! (Bitte versuchen Sie es erneut)", Severity.Error);
                else ShowSnackbar($"Fehler: {ex.Message}", Severity.Error);
            }
        }

        private void ValidateBeforeSave()
        {
            foreach (var level in _levels)
            {
                if ((level.AngabeUpdated || level.IsNew) && level.NewAngabe_PDF == null)
                    throw new Exception($"Level {level.Levelnr}: PDF fehlt!");

                foreach (var task in level.Aufgaben)
                {
                    if (task.IsNew && (task.NewInput_TXT == null || task.NewErgebnis_TXT == null))
                        throw new Exception($"Level {level.Levelnr}/Aufgabe {task.Aufgabennr}: Dateien fehlen!");
                }
            }
        }

        private async Task ProcessDatabaseOperations()
        {
            foreach (var level in _levels)
            {
                if (level.IsNew)
                {
                    await AddNewLevelToDb(level);
                }
                else
                {
                    await UpdateExistingLevel(level);
                }
            }
        }

        private async Task AddNewLevelToDb(LevelViewModel level)
        {
            var newLevel = new Level
            {
                Levelnr = level.Levelnr,
                Event_EventID = _selectedEvent.EventID,
                Angabe_PDF = level.NewAngabe_PDF,
                Aufgaben = level.Aufgaben.Select(a => new Aufgabe
                {
                    Aufgabennr = a.Aufgabennr,
                    Input_TXT = a.NewInput_TXT,
                    Ergebnis_TXT = a.NewErgebnis_TXT
                }).ToList()
            };
            dbContext.Levels.Add(newLevel);
        }

        private async Task UpdateExistingLevel(LevelViewModel level)
        {
            var dbLevel = await dbContext.Levels
                .Include(l => l.Aufgaben)
                .FirstAsync(l => l.LevelID == level.LevelId);

            if (level.AngabeUpdated)
                dbLevel.Angabe_PDF = level.NewAngabe_PDF;

            foreach (var task in level.Aufgaben)
            {
                if (task.IsNew)
                {
                    dbLevel.Aufgaben.Add(new Aufgabe
                    {
                        Aufgabennr = task.Aufgabennr,
                        Input_TXT = task.NewInput_TXT,
                        Ergebnis_TXT = task.NewErgebnis_TXT
                    });
                }
                else
                {
                    var dbTask = dbLevel.Aufgaben.First(a => a.AufgabenID == task.AufgabenId);
                    if (task.InputUpdated) dbTask.Input_TXT = task.NewInput_TXT;
                    if (task.ErgebnisUpdated) dbTask.Ergebnis_TXT = task.NewErgebnis_TXT;
                }
            }
        }

        private void ReorderLevels()
        {
            for (var i = 0; i < _levels.Count; i++)
                _levels[i].Levelnr = i + 1;
        }

        private async Task<byte[]> ConvertToBytesAsync(IBrowserFile file)
        {
            await using var stream = file.OpenReadStream(5 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        private void ShowSnackbar(string message, Severity severity)
            => Snackbar.Add(message, severity);
    }

    public class LevelViewModel
    {
        public int LevelId { get; set; }
        public int Levelnr { get; set; }
        public bool IsExpanded { get; set; }
        public List<AufgabeViewModel> Aufgaben { get; set; } = new();
        public byte[]? NewAngabe_PDF { get; set; }
        public bool AngabeUpdated { get; set; }
        public bool IsNew => LevelId == 0;
    }

    public class AufgabeViewModel
    {
        public int AufgabenId { get; set; }
        public int Aufgabennr { get; set; }
        public bool IsExpanded { get; set; }
        public byte[]? NewInput_TXT { get; set; }
        public byte[]? NewErgebnis_TXT { get; set; }
        public bool InputUpdated { get; set; }
        public bool ErgebnisUpdated { get; set; }
        public bool IsNew => AufgabenId == 0;
    }
}