// EventLog.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VCC_Projekt.Components.Pages
{
    public partial class EventLog
    {
        private class EventLogViewModel
        {
            public int EventLogID { get; set; }
            public string Tabellenname { get; set; }
            public string Beschreibung { get; set; }
            public DateTime Zeit { get; set; }
            public string KategorieBeschreibung { get; set; }
        }

        private List<EventLogViewModel> eventLogViewModels = new();
        private string _searchString = "";


        // Quick filter function that works across multiple columns
        private Func<EventLogViewModel, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Tabellenname?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (x.Beschreibung?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (x.KategorieBeschreibung?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (x.EventLogID.ToString().Contains(_searchString))
                return true;

            if (x.Zeit.ToString("dd.MM.yyyy HH:mm:ss").Contains(_searchString))
                return true;

            return false;
        };

        protected override void OnInitialized()
        {
            // Use a projection to get both the log and category information
            eventLogViewModels = dbContext.Set<Data.EventLog>()
                .Include(e => e.LogKat)
                .Select(e => new EventLogViewModel
                {
                    EventLogID = e.EventLogID,
                    Tabellenname = e.Tabellenname,
                    Beschreibung = e.Beschreibung,
                    Zeit = e.Zeit,
                    KategorieBeschreibung = e.LogKat != null ? e.LogKat.Beschreibung : "N/A"
                })
                .ToList();
        }
    }
}