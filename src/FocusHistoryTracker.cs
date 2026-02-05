using System;
using System.Collections.Generic;
using System.Linq;

namespace TransparentClock
{
    public static class FocusHistoryTracker
    {
        public static void AddFocusMinutes(AppState state, int minutes)
        {
            if (state == null)
            {
                return;
            }

            minutes = Math.Max(0, minutes);
            if (minutes == 0)
            {
                return;
            }

            DateTime today = DateTime.Today;
            var history = state.FocusHistory;

            if (!history.TryGetValue(today, out FocusDay day))
            {
                day = new FocusDay { Date = today, TotalFocusMinutes = 0 };
                history[today] = day;
            }

            day.TotalFocusMinutes += minutes;

            DateTime cutoff = today.AddDays(-6);
            List<DateTime> toRemove = history.Keys.Where(date => date < cutoff).ToList();
            foreach (var date in toRemove)
            {
                history.Remove(date);
            }

            AppStateStorage.Save(state);
        }
    }
}
