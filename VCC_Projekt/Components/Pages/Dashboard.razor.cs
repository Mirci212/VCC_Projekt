using Microsoft.AspNetCore.Components;
using VCC_Projekt.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Timers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using VCC_Projekt.Components.Account.Pages;

namespace VCC_Projekt.Components.Pages
{
    public partial class Dashboard : IDisposable
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Parameter]
        public int? EventId { get; set; }

        private List<Event> _events = new();
        private Event _selectedEvent;
        private List<RanglisteResult> _rankingList = new();
        private List<Participants> _participants = new();
        private System.Timers.Timer _refreshTimer;
        private bool isRanking = false;
        private bool accessDenied;
        private string accessDeniedMessage = "";

        protected override void OnInitialized()
        {
            try
            {
                var now = DateTime.Now;
                _events = dbContext.Events
                    .Where(x => x.Beginn <= now)
                    .OrderByDescending(ev => ev.Beginn)
                    .ToList();

                if (_events.Count > 0)
                {
                    // If we have an EventId parameter, try to find that event
                    if (EventId.HasValue)
                    {
                        var eventFromUrl = _events.FirstOrDefault(e => e.EventID == EventId.Value);
                        if (eventFromUrl != null)
                        {
                            _selectedEvent = eventFromUrl;
                        }
                        else
                        {
                            // If the specified event doesn't exist, set access denied
                            accessDenied = true;
                            accessDeniedMessage = $"Event mit ID {EventId.Value} existiert nicht oder ist noch nicht begonnen.";
                            return; // Early return to stop further processing
                        }
                    }
                    else
                    {
                        // No event specified in URL, use the first event
                        _selectedEvent = _events[0];

                        // Only redirect if we haven't encountered an error
                        if (!accessDenied)
                        {
                            NavigationManager.NavigateTo($"/dashboard/{_selectedEvent.EventID}", false);
                        }
                    }

                    // Only load ranking if we haven't encountered an error
                    if (!accessDenied)
                    {
                        LoadRanking(_selectedEvent.EventID);
                    }
                }
                else
                {
                    // No events found
                    accessDenied = true;
                    accessDeniedMessage = "Keine Veranstaltungen finden derzeit statt.";
                    return; // Early return
                }

                // Only initialize timer if access is not denied
                if (!accessDenied)
                {
                    InitializeRefreshTimer();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading events: {ex.Message}");
                _events = new List<Event>();
                accessDenied = true;
                accessDeniedMessage = $"Fehler: {ex.Message}";
                // Do not start timer
            }
        }

        private void InitializeRefreshTimer()
        {
            // Clean up any existing timer
            _refreshTimer?.Dispose();

            // Create new timer
            _refreshTimer = new System.Timers.Timer(5000);
            _refreshTimer.Elapsed += RefreshRanking;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        protected override void OnParametersSet()
        {
            // Reset access denied with each parameter change
            if (EventId.HasValue && _events.Any())
            {
                var eventFromUrl = _events.FirstOrDefault(e => e.EventID == EventId.Value);
                if (eventFromUrl != null)
                {
                    accessDenied = false; // Reset access denied status

                    if (_selectedEvent == null || _selectedEvent.EventID != eventFromUrl.EventID)
                    {
                        _selectedEvent = eventFromUrl;
                        LoadRanking(_selectedEvent.EventID);

                        // Make sure timer is running
                        if (_refreshTimer == null || !_refreshTimer.Enabled)
                        {
                            InitializeRefreshTimer();
                        }
                    }
                }
                else
                {
                    // Event ID exists in URL but event not found
                    accessDenied = true;
                    accessDeniedMessage = $"Event mit ID {EventId.Value} existiert nicht oder hat noch nicht begonnen.";
                    StopTimer();
                }
            }
        }

        private void StopTimer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Enabled = false;
            }
        }

        private async void RefreshRanking(object sender, ElapsedEventArgs e)
        {
            // Only refresh if not in access denied state
            if (!accessDenied && _selectedEvent != null)
            {
                await InvokeAsync(() =>
                {
                    try
                    {
                        LoadRanking(_selectedEvent.EventID);
                        StateHasChanged();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error refreshing ranking: {ex.Message}");
                        accessDenied = true;
                        accessDeniedMessage = $"Fehler beim Aktualisieren: {ex.Message}";
                        StopTimer();
                        StateHasChanged();
                    }
                });
            }
            else
            {
                // If we're in access denied state, stop refreshing
                StopTimer();
            }
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
                accessDenied = true;
                accessDeniedMessage = $"Fehler beim Laden der Rangliste: {ex.Message}";
                StopTimer();
            }
        }

        public void Dispose()
        {
            // Dispose of the timer when the component is disposed
            if (_refreshTimer != null)
            {
                _refreshTimer.Enabled = false;
                _refreshTimer.Elapsed -= RefreshRanking;
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }
    }
}