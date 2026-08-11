namespace OodHelper.Uno.Presentation;

/// <summary>
/// View-model for the shell page. The tab shell itself is driven from <c>MainPage</c> code-behind
/// (mirroring the WPF app's code-behind window), so this view-model only carries the window title.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public string Title => "OodHelper (Uno) — Walking Skeleton";

    public MainViewModel()
    {
    }
}
