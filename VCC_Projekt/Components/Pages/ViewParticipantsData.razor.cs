using Microsoft.AspNet.Identity;
using VCC_Projekt.Data;
using System.Linq;
using VCC_Projekt.Components.Account.Pages.Manage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
namespace VCC_Projekt.Components.Pages
{
    public partial class ViewParticipantsData
    {
        private Event _selectedEvent = new() { EventID = 0 };
        private List<Event> _events = new();
        private List<Participants> _participants = new();
        private string _searchString;
        private bool _isLoading = false;

        protected override void OnInitialized()
        {
            try
            {
                _events = dbContext.Events.OrderByDescending(ev => ev.Beginn).ToList();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error loading events: {ex.Message}");
                _events = new List<Event>();
            }
        }

        private async Task OnEventSelected(Event selectedEvent)
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                _selectedEvent = selectedEvent;
                if (_selectedEvent.EventID != 0)
                {
                    // Load participants for the selected event
                    var groups = dbContext.Gruppen
                        .Where(g => g.Event_EventID == _selectedEvent.EventID)
                        .Select(g => new Participants
                        {
                            Name = g.Gruppenname ?? g.GruppenleiterId,
                            Type = g.Teilnehmertyp,
                            Members = g.UserInGruppe.Select(u => new MemberInfo
                            {
                                User = u.User,
                                MemberType = u.User.Id == g.GruppenleiterId ? "Gruppenleiter" : "Teammitglied"
                            }).ToList()
                        })
                        .ToList();

                    var individualUsers = dbContext.UserInGruppe
                        .Where(u => u.Gruppe.Event_EventID == _selectedEvent.EventID && u.Gruppe.Teilnehmertyp == "Einzelspieler")
                        .Select(u => new Participants
                        {
                            Name = u.User.UserName,
                            Type = "Einzelspieler",
                            Members = new List<MemberInfo>
                            {
                                new MemberInfo
                                {
                                    User = u.User,
                                    MemberType = "Einzelspieler"
                                }
                            }
                        })
                        .ToList();

                    _participants = groups.Concat(individualUsers).ToList();
                }
                else
                {
                    _participants.Clear();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error loading participants: {ex.Message}");
                _participants = new List<Participants>();
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        // Search filter logic - fixed
        private Func<Participants, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString)) return true; // Show all if empty

            var lowerSearch = _searchString.ToLower();

            // Safer null checking and fixed nested Any logic
            return x.Name?.ToLower().Contains(lowerSearch) == true ||
                   x.Type?.ToLower().Contains(lowerSearch) == true ||
                   (x.Members != null && x.Members.Any(m =>
                       m.User?.UserName?.ToLower().Contains(lowerSearch) == true ||
                       m.MemberType?.ToLower().Contains(lowerSearch) == true));
        };
    }

    public class Participants
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Team or Einzelspieler
        public List<MemberInfo>? Members { get; set; } // Nullable
    }

    public class MemberInfo
    {
        public ApplicationUser User { get; set; }
        public string MemberType { get; set; } = string.Empty; // Gruppenleiter or Teammitglied
    }
}