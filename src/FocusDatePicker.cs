using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TransparentClock
{
    /// <summary>
    /// Custom DateTimePicker that validates and restricts dates to those with focus data
    /// </summary>
    public class FocusDatePicker : DateTimePicker
    {
        private HashSet<DateTime> validDates = new HashSet<DateTime>();
        private DateTime lastValidDate = DateTime.Today;
        private bool isUpdatingDate = false;

        public FocusDatePicker()
        {
            Format = DateTimePickerFormat.Short;
            ShowCheckBox = false;
            CalendarMonthBackground = Color.White;
            CalendarForeColor = Color.Black;
            CalendarTitleBackColor = Color.FromArgb(66, 139, 244);
            CalendarTitleForeColor = Color.White;
            CalendarTrailingForeColor = Color.Gray;
        }

        /// <summary>
        /// Set the dates that have focus data and should be enabled
        /// </summary>
        public void SetValidDates(List<DateTime> dates)
        {
            validDates = new HashSet<DateTime>(dates.Select(d => d.Date));
            
            // If current date is invalid, select the most recent valid date
            if (!validDates.Contains(Value.Date) && validDates.Count > 0)
            {
                isUpdatingDate = true;
                Value = validDates.OrderByDescending(d => d).First();
                isUpdatingDate = false;
                lastValidDate = Value.Date;
            }
        }

        /// <summary>
        /// Get the currently selected date
        /// </summary>
        public DateTime SelectedDate => Value.Date;

        protected override void OnValueChanged(EventArgs eventargs)
        {
            if (isUpdatingDate)
            {
                base.OnValueChanged(eventargs);
                return;
            }

            // If user selected an invalid date, revert to the last valid one
            if (!validDates.Contains(Value.Date) && validDates.Count > 0)
            {
                isUpdatingDate = true;
                Value = lastValidDate;
                isUpdatingDate = false;
                return;
            }

            if (validDates.Contains(Value.Date))
            {
                lastValidDate = Value.Date;
            }

            base.OnValueChanged(eventargs);
        }

        protected override void OnCloseUp(EventArgs eventargs)
        {
            // After calendar closes, ensure we have a valid date
            if (!validDates.Contains(Value.Date) && validDates.Count > 0)
            {
                isUpdatingDate = true;
                Value = lastValidDate;
                isUpdatingDate = false;
            }

            base.OnCloseUp(eventargs);
        }
    }
}
