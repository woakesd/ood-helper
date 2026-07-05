using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for EntrySheets.xaml
    /// </summary>
    public partial class EntrySheet : Page
    {
        public EntrySheet(int rid)
        {
            InitializeComponent();
            // This page is not constructed through DI, so its repository is resolved from the
            // container here, mirroring the other non-DI'd print screens.
            var c = App.Services.GetRequiredService<ICalendarRepository>().Get(rid)!;
            EventName.Text = c.Event;
            ClassName.Text = c.Class;
            DateTime? dt = c.StartDate;
            StartDate.Text = dt!.Value.ToString("dd MMM yyyy");
            StartTime.Text = dt.Value.ToString("HH:mm");
            switch (c.TimeLimitType)
            {
                case "F":
                    Fixed.Visibility = Visibility.Visible;
                    Delta.Visibility = Visibility.Collapsed;
                    if (c.TimeLimitFixed.HasValue)
                        TimeLimit.Text = c.TimeLimitFixed.Value.ToString("HH:mm");
                    break;
                case "D":
                    Fixed.Visibility = Visibility.Collapsed;
                    Delta.Visibility = Visibility.Visible;
                    if (c.TimeLimitDelta.HasValue)
                        TimeLimit.Text = (new TimeSpan(0, 0, c.TimeLimitDelta.Value)).ToString("hh\\:mm");
                    break;
                default:
                    Fixed.Visibility = Visibility.Collapsed;
                    Delta.Visibility = Visibility.Collapsed;
                    TimeLimit.Text = "No Time Limit";
                    break;
            }
            if (c.Extension.HasValue)
                Extension.Text = (new TimeSpan(0, 0, c.Extension.Value)).ToString("hh\\:mm");
            else
                Extension.Text = "No Extension";
            OOD.Text = c.Ood;
        }
    }
}
