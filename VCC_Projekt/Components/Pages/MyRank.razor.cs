using Microsoft.AspNetCore.Components;
using VCC_Projekt.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace VCC_Projekt.Components.Pages
{
    public partial class MyRank
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [Parameter]
        public int EventId { get; set; }

        private Event _event;
        private List<RanglisteResult> _topRankingList = new();
        private RanglisteResult _userRankingEntry;
        private bool showUserEntry = false;
        private bool accessDenied;
        private string accessDeniedMessage = "";
        private string _userId;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Get current user ID from authentication state
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated != true)
                {
                    accessDenied = true;
                    accessDeniedMessage = "Sie müssen angemeldet sein, um die Ergebnisse zu sehen.";
                    return;
                }

                // Get the user ID from claims
                _userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(_userId))
                {
                    accessDenied = true;
                    accessDeniedMessage = "Benutzer-ID konnte nicht ermittelt werden.";
                    return;
                }

                // Fetch the event
                _event = dbContext.Events.FirstOrDefault(e => e.EventID == EventId);

                if (_event == null)
                {
                    accessDenied = true;
                    accessDeniedMessage = $"Event mit ID {EventId} wurde nicht gefunden.";
                    return;
                }

                // Load the ranking and user position
                LoadRankingAndUserPosition();
            }
            catch (Exception ex)
            {
                accessDenied = true;
                accessDeniedMessage = $"Fehler beim Laden der Ergebnisse: {ex.Message}";
            }
        }

        protected override void OnParametersSet()
        {
            if (!accessDenied && (_event == null || _event.EventID != EventId))
            {
                // Reload when the event ID changes
                _ = OnInitializedAsync();
            }
        }

        private void LoadRankingAndUserPosition()
        {
            try
            {
                // Fetch complete ranking data for the event
                var completeRanking = dbContext.Set<RanglisteResult>()
                    .FromSqlRaw("CALL ShowRangliste(@eventId)", new MySqlParameter("@eventId", EventId))
                    .ToList();

                // Get top 10 entries
                _topRankingList = completeRanking.ToList();

                // Find user's group ID(s)
                //var userGroupIds = dbContext.UserInGruppe
                //    .Where(uig => uig.User.Id == _userId && uig.Gruppe.Event_EventID == EventId)
                //    .Select(uig => uig.Gruppe.GruppenID)
                //    .ToList();

                // Find user's ranking entry
                //_userRankingEntry = completeRanking.FirstOrDefault(r => userGroupIds.Contains(r.GruppenID));

                //// Check if user entry exists and if it's not already in top 10
                showUserEntry = false; /* _userRankingEntry != null && !_topRankingList.Any(r => r.GruppenID == _userRankingEntry.GruppenID);*/
            }
            catch (Exception ex)
            {
                accessDenied = true;
                accessDeniedMessage = $"Fehler beim Laden der Rangliste: {ex.Message}";
            }
        }

        private bool IsUserEntry(RanglisteResult entry)
        {
            if (_userRankingEntry == null)
                return false;

            return entry.GruppenID == _userRankingEntry.GruppenID;
        }
    }
}