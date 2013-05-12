using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OodHelper.Messaging;
using OodHelper.Results.ViewModel;

namespace OodHelper.Results.View
{
    /// <summary>
    /// Interaction logic for ResultsEditorView.xaml
    /// </summary>
    public partial class ResultEditorView : UserControl
    {
        public ResultEditorView()
        {
            InitializeComponent();
            Messenger.Default.Register<EditBoatMessage>(this, EditBoat);
        }

        public void EditBoat(EditBoatMessage Message)
        {
        }
    }
}
