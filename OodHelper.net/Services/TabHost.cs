using System;
using System.Windows.Controls;

namespace OodHelper.Services
{
    internal sealed class TabHost : ITabHost
    {
        private TabControl _tabControl = null!;

        public void Attach(TabControl tabControl)
        {
            _tabControl = tabControl;
        }

        public void AddTab(string header, object content)
        {
            if (_tabControl == null)
                throw new InvalidOperationException("TabHost has not been attached to a TabControl.");

            var tab = new TabItem { Content = content, Header = header };
            _tabControl.Items.Add(tab);
            _tabControl.SelectedItem = tab;
        }
    }
}
