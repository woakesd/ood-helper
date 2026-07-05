using System.Windows.Controls;

namespace OodHelper.Services
{
    public interface ITabHost
    {
        void Attach(TabControl tabControl);
        void AddTab(string header, object content);
    }
}
