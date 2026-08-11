using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using OodHelper.Uno;
using OodHelper.Uno.Presentation;
using OodHelper.ViewModels;

namespace OodHelper.Services
{
    /// <summary>
    /// WinUI/Uno implementation of <see cref="IDialogService"/>. The WPF version showed modal
    /// <c>Window</c>s and blocking <c>MessageBox</c>es; here everything is a <see cref="ContentDialog"/>
    /// shown asynchronously against the main window's <c>XamlRoot</c>. Picker dialogs host the ported
    /// views and close in response to their view-model's <c>CloseRequested</c> event.
    /// </summary>
    internal sealed class DialogService : IDialogService
    {
        private readonly IServiceProvider _services;

        public DialogService(IServiceProvider services)
        {
            _services = services;
        }

        private static XamlRoot? XamlRoot => App.MainWindowInstance?.Content?.XamlRoot;

        public Task ShowErrorAsync(string message, string caption) => ShowMessageAsync(caption, message);

        public Task ShowInformationAsync(string message, string caption) => ShowMessageAsync(caption, message);

        private async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
        }

        public async Task<bool?> ConfirmYesNoCancelAsync(string message, string caption)
        {
            var dialog = new ContentDialog
            {
                Title = caption,
                Content = message,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot,
            };

            return await dialog.ShowAsync() switch
            {
                ContentDialogResult.Primary => true,
                ContentDialogResult.Secondary => false,
                _ => (bool?)null,
            };
        }

        public Task ShowSeriesAsync()
        {
            var vm = _services.GetRequiredService<SeriesViewModel>();
            vm.Load();
            var view = new SeriesView { DataContext = vm };
            // The series list has no intrinsic close command, so it is dismissed via the chrome Close button.
            return ShowModalOverlayAsync("Series", view, subscribeCloseRequested: null);
        }

        public Task<bool> ShowSeriesEditorAsync(int sid)
        {
            var vm = _services.GetRequiredService<Func<int, SeriesEditViewModel>>()(sid);
            var view = new SeriesEditView { DataContext = vm };
            // The series editor supplies its own Save/Cancel (which raise CloseRequested); the modal
            // chrome's Close button is a fallback, mapped to a cancel. The bool result (Save = true)
            // lets the caller list refresh.
            return ShowModalOverlayAsync(sid == 0 ? "New series" : "Series editor", view,
                h => vm.CloseRequested += h);
        }

        public Task ShowHandicapsAsync()
        {
            var vm = _services.GetRequiredService<HandicapsViewModel>();
            vm.Load();
            var view = new HandicapsView { DataContext = vm };
            // Handicaps is a list screen with no intrinsic close command, so it is dismissed purely
            // via the modal chrome's Close button.
            return ShowModalOverlayAsync("Handicaps", view, subscribeCloseRequested: null);
        }

        public Task<bool> ShowSeriesRaceSelectAsync(int sid)
        {
            var vm = _services.GetRequiredService<Func<int, SeriesRaceSelectViewModel>>()(sid);
            vm.Load();
            var view = new SeriesRaceSelectView { DataContext = vm };
            return HostViewModelDialogAsync("Select Races", view, h => vm.CloseRequested += h);
        }

        public async Task<Guid?> ShowClassPickerAsync()
        {
            var vm = _services.GetRequiredService<SelectClassViewModel>();
            vm.Load();
            var view = new SelectClassView { DataContext = vm };
            var accepted = await HostViewModelDialogAsync("Pick a Class", view, h => vm.CloseRequested += h);
            return accepted ? vm.SelectedId : null;
        }

        public Task<bool> ShowHandicapEditorAsync(Guid? id)
        {
            // The handicap (class) editor is not part of the walking skeleton. Surface a notice so the
            // Handicaps screen's Add/Edit buttons are still discoverable, and report "not saved".
            return ShowMessageAsync("Not yet available",
                "The class editor has not been ported to Uno yet.").ContinueWith(_ => false);
        }

        /// <summary>
        /// Hosts a full screen (<paramref name="content"/>) in a modal <see cref="Popup"/> overlay —
        /// a dimmed backdrop plus a centred card with a title and a Close button. This is the desktop
        /// stand-in for the WPF modal <c>Window</c>s used by the Series and Handicaps editors. A
        /// <see cref="Popup"/> is used rather than a <see cref="ContentDialog"/> so that the hosted
        /// screen can itself raise nested <see cref="ContentDialog"/>s (validation, confirm, the
        /// race-select picker) without hitting WinUI's "only one ContentDialog open at a time" rule.
        /// Resolves when the screen is dismissed.
        /// </summary>
        private static Task<bool> ShowModalOverlayAsync(string title, FrameworkElement content,
            Action<Action<bool>>? subscribeCloseRequested)
        {
            var xamlRoot = XamlRoot;
            var tcs = new TaskCompletionSource<bool>();

            var titleBlock = new TextBlock
            {
                Text = title,
                Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"],
                VerticalAlignment = VerticalAlignment.Center,
            };
            var closeButton = new Button { Content = "Close", HorizontalAlignment = HorizontalAlignment.Right };

            var header = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(closeButton, 1);
            header.Children.Add(titleBlock);
            header.Children.Add(closeButton);

            var body = new Grid();
            body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(content, 1);
            body.Children.Add(header);
            body.Children.Add(content);

            var card = new Border
            {
                Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = body,
            };

            // A hit-testable dimmed backdrop that covers the whole window, blocking input to the
            // content behind it (modal feel).
            var overlay = new Grid
            {
                Background = new SolidColorBrush(new Windows.UI.Color { A = 0x99, R = 0, G = 0, B = 0 }),
            };
            overlay.Children.Add(card);

            var popup = new Popup { XamlRoot = xamlRoot, Child = overlay };

            void Resize()
            {
                if (xamlRoot is null) return;
                overlay.Width = xamlRoot.Size.Width;
                overlay.Height = xamlRoot.Size.Height;
                card.MaxWidth = xamlRoot.Size.Width * 0.9;
                card.MaxHeight = xamlRoot.Size.Height * 0.9;
            }

            void OnXamlRootChanged(XamlRoot sender, XamlRootChangedEventArgs args) => Resize();

            void Close(bool result)
            {
                if (tcs.Task.IsCompleted) return;
                if (xamlRoot != null) xamlRoot.Changed -= OnXamlRootChanged;
                popup.IsOpen = false;
                tcs.TrySetResult(result);
            }

            closeButton.Click += (_, _) => Close(false);
            subscribeCloseRequested?.Invoke(Close);

            Resize();
            if (xamlRoot != null) xamlRoot.Changed += OnXamlRootChanged;
            popup.IsOpen = true;
            return tcs.Task;
        }

        /// <summary>
        /// Shows <paramref name="content"/> in a button-less <see cref="ContentDialog"/> and resolves
        /// when the hosted view-model raises its <c>CloseRequested</c> event (true = accepted).
        /// </summary>
        private static Task<bool> HostViewModelDialogAsync(string title, UIElement content,
            Action<Action<bool>> subscribeCloseRequested)
        {
            var tcs = new TaskCompletionSource<bool>();
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                XamlRoot = XamlRoot,
            };

            subscribeCloseRequested(result =>
            {
                tcs.TrySetResult(result);
                dialog.Hide();
            });

            _ = dialog.ShowAsync();
            return tcs.Task;
        }
    }
}
