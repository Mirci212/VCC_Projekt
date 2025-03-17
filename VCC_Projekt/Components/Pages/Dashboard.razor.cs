using Microsoft.AspNetCore.Components;
using VCC_Projekt.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Timers;
using System.Threading.Tasks;

namespace VCC_Projekt.Components.Pages
{
    public partial class Dashboard : IDisposable
    {
        private List<Event> _events = new();
        private Event _selectedEvent;
        private List<RanglisteResult> _rankingList = new();
        private List<Participants> _participants = new();
        private System.Timers.Timer _refreshTimer;
        private bool isRanking = false; // New variable to check if ranking is populated

        protected override void OnInitialized()
        {
            try
            {
                // TODO
                var now = DateTime.Now;
                _events = dbContext.Events
                    .Where(x => x.Beginn <= now && x.Beginn.AddMinutes(x.Dauer) > now)
                    .OrderByDescending(ev => ev.Beginn)
                    .ToList();

                // Automatically select the first event if there are multiple events
                if (_events.Count > 0)
                {
                    _selectedEvent = _events[0];
                    LoadRanking(_selectedEvent.EventID); // Load ranking for the default selected event
                }

                // Initialize and start the timer to refresh the ranking every 5 seconds
                _refreshTimer = new System.Timers.Timer(5000); // 5000 milliseconds = 5 seconds
                _refreshTimer.Elapsed += RefreshRanking;
                _refreshTimer.AutoReset = true;
                _refreshTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading events: {ex.Message}");
                _events = new List<Event>();
            }
        }

        private async void RefreshRanking(object sender, ElapsedEventArgs e)
        {
            if (_selectedEvent != null)
            {
                await InvokeAsync(() =>
                {
                    LoadRanking(_selectedEvent.EventID);
                    StateHasChanged(); // Notify the component that the state has changed
                });
            }
        }

        private async Task OnEventSelected(Event selectedEvent)
        {
            _selectedEvent = selectedEvent; // Store the selected event
            LoadRanking(selectedEvent.EventID); // Load ranking for the newly selected event
        }

        private void LoadRanking(int eventId)
        {
            try
            {
                // Fetch ranking data for the selected event
                var rankingData = dbContext.Set<RanglisteResult>()
                    .FromSqlRaw("CALL ShowRangliste(@eventId)", new MySqlParameter("@eventId", eventId))
                    .ToList();

                // Fetch all participants (groups and individual users) for the event
                var groups = dbContext.Gruppen
                    .Where(g => g.Event_EventID == eventId)
                    .Select(g => new RanglisteResult
                    {
                        GruppenID = g.GruppenID,
                        Gruppenname = g.Gruppenname,
                        GruppenleiterId = g.GruppenleiterId,
                        Teilnehmertyp = g.Teilnehmertyp,
                        AbgeschlosseneLevel = "", // Default to 0 for participants who haven't completed any levels
                        AnzahlLevel = 0, // Default to 0
                        GesamtFehlversuche = 0, // Default to 0
                        MaxBenötigteZeit = null, // No time data
                        GebrauchteZeit = null // No time data
                    })
                    .ToList();

                var individualUsers = dbContext.UserInGruppe
                    .Where(u => u.Gruppe.Event_EventID == eventId && u.Gruppe.Teilnehmertyp == "Einzelspieler")
                    .Select(u => new RanglisteResult
                    {
                        GruppenID = u.Gruppe.GruppenID,
                        Gruppenname = null, // Individual players have no group name
                        GruppenleiterId = u.User.Id,
                        Teilnehmertyp = "Einzelspieler",
                        AbgeschlosseneLevel = "", // Default to 0
                        AnzahlLevel = 0, // Default to 0
                        GesamtFehlversuche = 0, // Default to 0
                        MaxBenötigteZeit = null, // No time data
                        GebrauchteZeit = null // No time data
                    })
                    .ToList();

                // Combine all participants
                var allParticipants = groups.Concat(individualUsers).ToList();

                // Merge ranking data with all participants
                var rankedParticipants = rankingData
                    .Select(ranking => new RanglisteResult
                    {
                        GruppenID = ranking.GruppenID,
                        Gruppenname = ranking.Gruppenname,
                        GruppenleiterId = ranking.GruppenleiterId,
                        Teilnehmertyp = ranking.Teilnehmertyp,
                        AbgeschlosseneLevel = ranking.AbgeschlosseneLevel,
                        AnzahlLevel = ranking.AnzahlLevel,
                        GesamtFehlversuche = ranking.GesamtFehlversuche,
                        MaxBenötigteZeit = ranking.MaxBenötigteZeit,
                        GebrauchteZeit = ranking.GebrauchteZeit,
                        Rang = ranking.Rang // Preserve the ranking
                    })
                    .ToList();

                // Find participants who haven't completed any levels
                var unrankedParticipants = allParticipants
                    .Where(participant => !rankingData.Any(ranking => ranking.GruppenID == participant.GruppenID))
                    .OrderBy(p => p.Gruppenname ?? p.GruppenleiterId.ToString()) // Sort alphabetically
                    .ToList();

                // Combine ranked and unranked participants
                _rankingList = rankedParticipants
                    .Concat(unrankedParticipants)
                    .ToList();

                // Assign ranks to unranked participants (starting after the last ranked participant)
                int lastRank = rankedParticipants.Count;
                for (int i = 0; i < unrankedParticipants.Count(); i++)
                {
                    unrankedParticipants[i].Rang = lastRank + i + 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ranking: {ex.Message}");
                _rankingList = new List<RanglisteResult>();
            }
        }

        public void Dispose()
        {
            // Dispose of the timer when the component is disposed
            _refreshTimer?.Dispose();
        }
    }
}