using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OodHelper.Services;

namespace OodHelper.Uno.Presentation
{
    /// <summary>
    /// The application shell: a menu over a welcome landing area, in the spirit of the WPF
    /// <c>OodHelperWindow</c>. The editor screens open as modal overlays via <see cref="IDialogService"/>,
    /// matching the WPF app's <c>ShowDialog&lt;Series&gt;</c>/<c>ShowDialog&lt;Handicaps&gt;</c> behaviour.
    /// View-models/services are resolved from the Uno.Extensions host via <see cref="App.Services"/>.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void OpenHandicaps_Click(object sender, RoutedEventArgs e)
        {
            var dialogs = App.Services.GetRequiredService<IDialogService>();
            await dialogs.ShowHandicapsAsync();
        }

        private async void OpenSeries_Click(object sender, RoutedEventArgs e)
        {
            var dialogs = App.Services.GetRequiredService<IDialogService>();
            await dialogs.ShowSeriesAsync();
        }

        private async void PickClass_Click(object sender, RoutedEventArgs e)
        {
            var dialogs = App.Services.GetRequiredService<IDialogService>();
            var id = await dialogs.ShowClassPickerAsync();
            await dialogs.ShowInformationAsync(
                id is null ? "No class was picked." : $"You picked class id {id}.",
                "Class picker result");
        }
    }
}
